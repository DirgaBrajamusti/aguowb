namespace MERCY.Web.BackEnd.Models
{
    public class Permission_Item
    {
        public bool Is_View { get; set; }
        public bool Is_Add { get; set; }
        public bool Is_Delete { get; set; }
        public bool Is_Edit { get; set; }
        public bool Is_Active { get; set; }
        public bool Is_Acknowledge { get; set; }
        public bool Is_Approve { get; set; }
        public bool Is_Email { get; set; }
    }
}