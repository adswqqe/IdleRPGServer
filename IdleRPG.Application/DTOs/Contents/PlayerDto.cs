namespace IdleRPG.Application.DTOs.Contents
{
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int Level { get; set; }
        public long Gold { get; set; }
    }
}