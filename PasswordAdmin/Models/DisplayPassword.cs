namespace PasswordAdmin.Models
{
    class DisplayPassword
    {
        private string type = "password";
        private string eye_src = "assets/closed-eye.png";
        private string eye_alt = "closed eye";

        public void ShowPassword()
        {
            if (type == "password")
            {
                type = "text";
                eye_src = "assets/open-eye.png";
                eye_alt = "open eye";
            }
            else
            {
                type = "password";
                eye_src = "assets/closed-eye.png";
                eye_alt = "closed eye";
            }
        }

        public string getType()
        {
            return type;
        }
        public string getEyeSrc()
        {
            return eye_src;
        }
        public string getEyeAlt()
        {
            return eye_alt;
        }
    }
}
