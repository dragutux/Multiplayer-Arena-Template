using UnityEngine;
using UnityEngine.UI;

namespace Client
{
    public class LoginManager : MonoBehaviour
    {
        public static LoginManager getInstance;
        void Awake() { getInstance = this; }

        [SerializeField] InputField nickInput, passwordInput;
        [SerializeField] Text log;

		//toggle save password
		//on login save in playerspref is the toggle is true
		//on change screen to this, load the password and username from playerspref if exist
        public void Login()
        {
            string nickname = nickInput.text;

            //string password = passwordInput.text;

            Clean();

            if (string.IsNullOrEmpty(nickname) /*|| string.IsNullOrEmpty(password)*/)
            {
                log.text = "Nick was empty!";
                return;
            }
            nickInput.text = "dragutux";

            // no strangulation of nicks
            /*
            if (!email.Contains("@"))
            {
                log.text = "Invalid email!";
                return;
            }
            */
            // remove password later
            if (nickname.Length < 2 /*|| password.Length < 2*/)
            {
                log.text = "Email or password is too small!";
                return;
            }

            Client.getInstance.Login(nickname/*, password*/);
        }

        public void LoginResponse(bool ok, string response)
        {
            Debug.Log("Login Response: " + response);

            if (ok)
                MenuManager.getInstance.Change(Screen.Lobby);
            else
                log.text = response;
        }

        void OnEnable()
        {
            Clean();
        }

        public void Clean()
        {
            nickInput.text =/* passwordInput.text = */log.text = string.Empty;
            nickInput.text = "dragutux";
        }

        public void Register()
        {
            MenuManager.getInstance.Change(Screen.Registration);
        }
    }
}