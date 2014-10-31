using System.Collections.Generic;

namespace MMBot
{
    public interface IUser
    {
        string Id { get; }
        string Name { get; }
        IEnumerable<string> Roles { get; set; }
        string Room { get; }
        string AdapterId { get; set; }
    }
}