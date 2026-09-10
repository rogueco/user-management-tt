using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace UserManagement.Models;

public class UserLog
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Forename { get; set; } = default!;
    public string Surname { get; set; } = default!;
    public string Email { get; set; } = default!;
    public UserLogAction Action { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Changes { get; set; }

    public IReadOnlyList<UserLogChange> GetChanges()
        => Changes is null ? [] : JsonSerializer.Deserialize<List<UserLogChange>>(Changes) ?? [];

    public void SetChanges(IReadOnlyList<UserLogChange> changes)
        => Changes = changes.Count == 0 ? null : JsonSerializer.Serialize(changes);
}
