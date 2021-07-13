namespace API.Entities
{
    public class Like
    {
        public AppUser LikingUser{get;set;}
        public int LikingUserId{get;set;}

        public AppUser LikedUser{get;set;}
        public int LikedUserId{get;set;}
    }
}