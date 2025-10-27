namespace Followers.API.Models
{
    public class Follow
    {
        public int FollowerId { get; set; }
        public int FollowingId { get; set; }
        public DateTime FollowedAt { get; set; }
    }
}
