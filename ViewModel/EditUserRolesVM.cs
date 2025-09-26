namespace ProyetoSetilPF.ViewModel
{
    public class EditUserRolesVM
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public List<RoleSelection> Roles { get; set; }
        public class RoleSelection
        {
            public string RoleName { get; set; }
            public bool Selected { get; set; }
        }
    }
}
