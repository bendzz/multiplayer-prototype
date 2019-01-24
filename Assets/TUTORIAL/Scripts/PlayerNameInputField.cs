using UnityEngine;
using UnityEngine.UI;

using System.Collections;

namespace Mike.QuickMP
{
    /// <summary>
    /// Player name input field. Let the user input his name, will appear above the player in the game.
    /// </summary>
    [RequireComponent(typeof(InputField))]
    public class PlayerNameInputField : MonoBehaviour
    {
        #region Private Variables

        // Store the PlayerPref Key to avoid typos
        static string playerNamePrefKey = "PlayerName";
        InputField _inputField;

        #endregion


        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {

            string defaultName = "";
            //InputField _inputField = this.GetComponent<InputField>();
            _inputField = this.GetComponent<InputField>();
            //if (_inputField != null)
            //{
                if (PlayerPrefs.HasKey(playerNamePrefKey))
                {
                    defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                    _inputField.text = defaultName;
                }
            //}


            PhotonNetwork.playerName = defaultName;
        }


        #endregion


        #region Public Methods


        /// <summary>
        /// Sets the name of the player and save it in the PlayerPrefs for future sessions.
        /// </summary>
        /// <param name="value">The name of the Player</param>
        //public void SetPlayerName(string value)
        public void SetPlayerName(InputField value)
        {
            //Note: 'value' not used

            string input = _inputField.text;

            // #Important
            //PhotonNetwork.playerName = value + " "; // force a trailing space string in case value is an empty string, else playerName would not be updated.

            //PlayerPrefs.SetString(playerNamePrefKey, value);
            //PlayerPrefs.SetString(playerNamePrefKey, value.text);

            // had to rewrite this function using this https://www.reddit.com/r/Unity3D/comments/66jc06/how_to_get_inputfield_value/dgjc1nc/  didn't get any text using the tutorial's method
            //Debug.Log("name set to " + value.text);

            PhotonNetwork.playerName = input + " "; // force a trailing space string in case value is an empty string, else playerName would not be updated.
            PlayerPrefs.SetString(playerNamePrefKey, input);
        }


        #endregion
    }
}