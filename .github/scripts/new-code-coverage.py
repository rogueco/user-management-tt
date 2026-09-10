"""Coverage of the executable lines a PR adds, from the coverlet reports under TestResults.

Usage: new-code-coverage.py <git diff range>   with FLOOR (percent) in the environment.
Test code never enters the denominator; files excluded in tests/coverlet.runsettings have
no coverage segments and so are not counted either. Lines with no segment (braces,
declarations, comments) are ignored, matching what the coverage total measures.
"""
import glob
import os
import re
import subprocess
import sys
import xml.etree.ElementTree as ET

diff_range = sys.argv[1]
floor = float(os.environ.get("FLOOR", "80"))
root = os.getcwd()

diff = subprocess.check_output(["git", "diff", "--unified=0", "--no-renames", diff_range, "--", "*.cs"], text=True)
changed = {}
current = None
for line in diff.splitlines():
    if line.startswith("+++ b/"):
        current = line[6:]
        changed.setdefault(current, set())
        continue
    if not line.startswith("@@") or current is None:
        continue
    match = re.search(r"\+(\d+)(?:,(\d+))?", line)
    if match:
        start = int(match.group(1))
        count = int(match.group(2) or "1")
        changed[current].update(range(start, start + count))
changed = {path: lines for path, lines in changed.items() if not path.startswith("tests/")}

executable = {}
for report in glob.glob("TestResults/**/coverage.cobertura.xml", recursive=True):
    tree = ET.parse(report).getroot()
    sources = [source.text for source in tree.iter("source") if source.text]
    for cls in tree.iter("class"):
        filename = cls.get("filename", "")
        candidates = [filename] if os.path.isabs(filename) else [os.path.join(source, filename) for source in sources]
        path = next((os.path.relpath(c, root) for c in candidates if os.path.exists(c)), None)
        lines = cls.find("lines")
        if path not in changed or lines is None:
            continue
        for line in lines:
            number = int(line.get("number"))
            if number in changed[path]:
                file_lines = executable.setdefault(path, {})
                file_lines[number] = file_lines.get(number, False) or int(line.get("hits", "0")) > 0

total = sum(len(lines) for lines in executable.values())
covered = sum(sum(1 for hit in lines.values() if hit) for lines in executable.values())
percent = (covered / total * 100.0) if total else 100.0
print(f"Changed executable lines: {covered}/{total} ({percent:.1f}%)")

details = []
for path, lines in sorted(executable.items()):
    file_covered = sum(1 for hit in lines.values() if hit)
    file_percent = file_covered / len(lines) * 100.0
    missed = [str(number) for number, hit in sorted(lines.items()) if not hit]
    print(f"  {file_covered}/{len(lines)} ({file_percent:.1f}%) {path}")
    if missed:
        print(f"    uncovered changed lines: {', '.join(missed)}")
    details.append((path, file_covered, len(lines), file_percent))

summary_path = os.environ.get("GITHUB_STEP_SUMMARY")
if summary_path:
    with open(summary_path, "a", encoding="utf-8") as summary:
        summary.write("### New-code coverage\n\n")
        summary.write(f"Changed executable lines: **{covered}/{total} ({percent:.1f}%)**\n\n")
        if details:
            summary.write("| File | Covered | Coverage |\n|---|---:|---:|\n")
            for path, file_covered, file_total, file_percent in details:
                summary.write(f"| `{path}` | {file_covered}/{file_total} | {file_percent:.1f}% |\n")

if total == 0:
    print("No changed executable lines found; new-code coverage is satisfied.")
    sys.exit(0)
if percent < floor:
    print(f"::error::New-code coverage {percent:.1f}% is below the floor of {floor:.1f}%.")
    sys.exit(1)
print(f"New-code coverage floor satisfied ({percent:.1f}% >= {floor:.1f}%).")
