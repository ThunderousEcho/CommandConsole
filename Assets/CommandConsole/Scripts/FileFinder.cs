using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Diagnostics;

public class FileFinder : MonoBehaviour {

    //Locates csc.exe and cscompui.dll version 3.5 in the user's machine and copies them into the game's/project's root directory. Integrates with Unity UGUI.

#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
    string rootToSearch = @"C:\Windows\Microsoft.NET\Framework\v3.5";
#elif (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
    string rootToSearch = "/usr/local/share/dotnet/dotnet";
#elif(UNITY_EDITOR || UNITY_STANDALONE_LINUX)
    string rootToSearch = "/usr/etc";
#else
    string rootToSearch = @"C:\";
#endif

    [HideInInspector] public Text searchingText;
    [HideInInspector] public Text error;
    [HideInInspector] public InputField path;

    string threadState;

    Thread findThread;
    string currentDir = "";

    bool threadAborted = true;

    FileInfo bestCsc; public long cscWanted = long.MinValue; string cscVersion; bool cscInstalled;
    FileInfo bestCscompui; public long cscompuiWanted = long.MinValue; string cscompuiVersion; bool cscompuiInstalled;

    [HideInInspector] public Text cscIndicator;
    [HideInInspector] public Text cscompuiIndicator;

    public void startFind() {
        if (findThread != null && findThread.IsAlive) {
            threadAborted = true;
            error.text = "The thread has not yet terminated. Please wait a few seconds.";
        } else {
            rootToSearch = path.text;
            if (Directory.Exists(rootToSearch)) {
                error.text = "";
                DirectoryInfo dir = new DirectoryInfo(rootToSearch);
                findThread = new Thread(() => find(() => findRecursive(dir), Handler));
                threadAborted = false;
                findThread.Start();
            } else {
                error.text = "'" + rootToSearch + @"' Does not exist. Try 'C:\'.";
            }
        }
    }

    void Start() {
        path.text = rootToSearch;
    }

    long evaluateHowMuchWanted(FileVersionInfo info) {
        bool greaterThan35 = false;
        if (info.FileMajorPart > 3)
            greaterThan35 = true;
        else if (info.FileMajorPart == 3 && info.FileMinorPart > 5)
            greaterThan35 = true;

        long diff = (info.FileMajorPart - 3);
        diff = diff << 16;
        diff += (info.FileMinorPart - 5);

        diff = -Math.Abs(diff);

        if (greaterThan35)
            diff -= 1 << 32;

        return diff;
    }

    void OnEnable() {
        threadAborted = true;

        if (File.Exists("csc.exe")) {
            checkNewBest(new FileInfo("csc.exe"), true);
        }
        if (File.Exists("cscompui.dll")) {
            checkNewBest(new FileInfo("cscompui.dll"), true);
        }
    }

    void OnDestroy() {
        OnDisable();
    }

    void OnDisable() {
        threadAborted = true;
    }

    void checkNewBest(FileInfo file, bool installed) {
        FileVersionInfo version = FileVersionInfo.GetVersionInfo(file.FullName);
        long value = evaluateHowMuchWanted(version);
        switch (file.Name) {
            case "csc.exe":
                if (value > cscWanted) {
                    cscWanted = value;
                    cscVersion = version.FileMajorPart + "." + version.FileMinorPart;
                    bestCsc = file;
                    cscInstalled = installed;
                }
                break;
            case "cscompui.dll":
                if (value > cscompuiWanted) {
                    cscompuiWanted = value;
                    cscompuiVersion = version.FileMajorPart + "." + version.FileMinorPart;
                    bestCscompui = file;
                    cscompuiInstalled = installed;
                }
                break;
            default:
                throw new Exception("File is not csc.exe or cscompui.dll!");
        }
    }

    public void Update() {

        if (cscWanted == 0 && cscompuiWanted == 0) { //both already 3.5
            threadAborted = true;
        }

        string wantedText = "";
        if (!threadAborted)
            wantedText = "Searching: " + currentDir;
        else
            wantedText = "Hopefully, you have version 3.5 of both files! If not, try again with a different path. Using anything but 3.5 may result in problems with C# commands.";
        if (!searchingText.text.Equals(wantedText)) {
            searchingText.text = wantedText;
        }

        string cscText = bestCsc == null ? "<color=red>NOT INSTALLED! | csc.exe</color>" : "version " + cscVersion + " found | csc.exe";
        if (cscInstalled)
            cscText = "version " + cscVersion + " installed | csc.exe";
        if (cscWanted == 0)
            cscText = "<color=#00ff00>" + cscText + "</color>";
        if (cscIndicator.text != cscText)
            cscIndicator.text = cscText;

        string cscompuiText = bestCscompui == null ? "<color=red>cscompui.dll | NOT INSTALLED!</color>" : "cscompui.dll | version " + cscompuiVersion + " found";
        if (cscompuiInstalled)
            cscompuiText = "cscompui.dll | version " + cscompuiVersion + " installed";
        if (cscompuiWanted == 0)
            cscompuiText = "<color=#00ff00>" + cscompuiText + "</color>";
        if (cscompuiIndicator.text != cscompuiText)
            cscompuiIndicator.text = cscompuiText;
    }

    private static void Handler(Exception exception) {
        UnityEngine.Debug.LogException(exception);
    }

    void find(Action findRecursive, Action<Exception> handler) {
        try {
            findRecursive.Invoke();

            if (bestCsc != null && !cscInstalled) {
                bestCsc.CopyTo("csc.exe", true);
                cscInstalled = true;
            }
            if (bestCscompui != null && !cscompuiInstalled) {
                bestCscompui.CopyTo("cscompui.dll", true);
                cscompuiInstalled = true;
            }

        } catch (Exception ex) {
            Handler(ex);
        }
        threadAborted = true;
    }

    void findRecursive(DirectoryInfo dir) {

        if (threadAborted)
            return;

        try {

            var files = dir.GetFiles("csc.exe", SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                checkNewBest(file, false);
            }

            files = dir.GetFiles("cscompui.dll", SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                checkNewBest(file, false);
            }

            foreach (var subdir in dir.GetDirectories()) {
                if (threadAborted)
                    return;

                currentDir = subdir.FullName;
                findRecursive(subdir);
            }

        } catch (UnauthorizedAccessException) { //files we're not allowed to look at
            return;
        }
    }
}
