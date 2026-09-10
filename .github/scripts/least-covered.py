"""Prints the 20 least-covered classes from a merged Cobertura report."""
import sys
import xml.etree.ElementTree as ET

rows = []
for cls in ET.parse(sys.argv[1]).getroot().iter("class"):
    lines = cls.find("lines")
    if lines is None or len(lines) == 0:
        continue
    covered = sum(1 for line in lines if int(line.get("hits", "0")) > 0)
    rows.append((covered / len(lines) * 100, covered, len(lines), cls.get("name")))

for percent, covered, total, name in sorted(rows)[:20]:
    print(f"{percent:5.1f}%  {covered:3d}/{total:<3d}  {name}")
