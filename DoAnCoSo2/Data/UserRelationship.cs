namespace DoAnCoSo2.Data
{
    public class UserRelationship
    {
        public string FollowerId { get; set; }
        public ApplicationUser Follower { get; set; }

        public string FolloweeId { get; set; }
        public ApplicationUser Followee { get; set; }
    }

}
