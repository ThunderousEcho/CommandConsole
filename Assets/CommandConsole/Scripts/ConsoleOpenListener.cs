using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CharlesOlinerCommandConsole {
    public class ConsoleOpenListener : MonoBehaviour {

        /* TODO:
         * autocomplete is member-specific instead of just grabbing every word ever
         * 
         * video / bug testing
         */

        //Since the console is disabled when it is closed, it can't tell when a user presses the open key.
        //This script enables the console whenever the open key is pressed.
        //(Should be placed on a parent of the console elements.)

        public KeyCode consleOpenKey = KeyCode.BackQuote; //the key that opens the console. Tilde (~) by default.
        public bool destroyOnLoad; //changing this value does nothing once Start() is called
        [HideInInspector] public CommandConsole console;
        [HideInInspector] public Text[] textsToClear; //the script also has to clear the tooltips because disabled objects don't trigger PointerExit.

        private void Start() {
            if (!console.unloadAutocompleteLibraryWhileClosed) { //console isn't enabled to do this for itself
                console.unloadAutocompleteLibraryWhileClosed = true;
                console.OnEnable();
                console.unloadAutocompleteLibraryWhileClosed = false;
            }
            if (!destroyOnLoad) {
                DontDestroyOnLoad(gameObject);
            }
        }

        void Update() {
            if (Input.GetKeyDown(consleOpenKey)) {
                transform.GetChild(0).gameObject.SetActive(true);
                foreach (Text t in textsToClear) {
                    t.text = "";
                }
            }
        }
    }
}