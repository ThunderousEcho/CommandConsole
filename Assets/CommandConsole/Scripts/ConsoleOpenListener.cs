using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace CharlesOlinerCommandConsole {
    public class ConsoleOpenListener : MonoBehaviour {

        //Since the console is disabled when it is closed, it can't tell when a user presses the open key.
        //This script enables the console whenever the open key is pressed.
        //(Should be placed on a parent of the console elements.)

        public KeyCode consleOpenKey = KeyCode.BackQuote; //the key that opens the console. Tilde (~) by default.

        public bool dontDestroyOnLoad; //if false, the console will be destroyed once you change scenes. changing this value does nothing once Start() is called

        public bool fullAutocompleteLibrary; //when checked, the library is loaded with every single thing you could possibly want to type, but most of it is useless and gets in the way of the stuff you actually want. the library only updates when unloaded and reloaded.
        public bool allowAutocomplete = true; //autocomplete takes about 4 MB if in full mode. do not change the value once Start() is called.
        public bool unloadAutocompleteLibraryWhileClosed; //will result in somewhat odd behaviour if it is changed while the console is closed.

        public string temporaryFilesPath; //e.g. c:\CommandConsoleTempFiles. changing this at runtime skips some sanity checks and may cause errors.
        public string pathToCscDotExeV3_5; //e.g. c:\Windows\Microsoft.NET\Framework\v3.5\csc.exe. changing this at runtime skips some sanity checks and may cause errors.

        public GameObject selectedObject; //selected object

        [HideInInspector] public CommandConsole console;
        [HideInInspector] public GameObject consoleUIParent;
        [HideInInspector] public Text[] textsToClear; //the script also has to clear the tooltips because disabled objects don't trigger PointerExit.

        void Start() {
            if (!unloadAutocompleteLibraryWhileClosed) { //console isn't enabled to do this for itself
                unloadAutocompleteLibraryWhileClosed = true;
                console.OnEnable();
                unloadAutocompleteLibraryWhileClosed = false;
            }
            if (dontDestroyOnLoad) {
                DontDestroyOnLoad(gameObject);
            }
        }

        void Update() {
            if (Input.GetKeyDown(consleOpenKey)) {
                consoleUIParent.SetActive(true);
                foreach (Text t in textsToClear) {
                    t.text = "";
                }
            }
        }
    }
}