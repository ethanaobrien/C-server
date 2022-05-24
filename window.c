#define NOMINMAX
#include <windows.h>
#include <winuser.h>
#include <commdlg.h>
#include <ShlObj.h>

const char g_szClassName[] = "myWindowClass";

void toggleServer() {
    if (Settings.isRunning) {
        main_server = makeServer();
    } else {
        pthread_cancel(main_server);
        closesocket(sock);
    }
}

int width = 410, height = 700;

HWND hwndButton,
     portInput,
     hwndChooseFolder,
     corsSetting,
     indexSetting,
     openButton,
     openGithub;

void paintWindow(HWND hwnd) {
    PAINTSTRUCT ps;
    HDC hdc = BeginPaint(hwnd, &ps);
    char msg[Settings.error?5:(Settings.isRunning?7:11)];
    memset(msg, '\0', sizeof(msg));
    if (Settings.error) {
        sprintf(msg, "Error");
        char portMsg[41+getIntTextLen(Settings.port)];
        sprintf(portMsg, "There was an error listening on the port %i", Settings.port);
        SetWindowText(openButton, TEXT(portMsg));
    } else if (Settings.isRunning) {
        char portMsg[38+getIntTextLen(Settings.port)];
        sprintf(portMsg, "Open http://127.0.0.1:%i in your browser", Settings.port);
        SetWindowText(openButton, TEXT(portMsg));
        sprintf(msg, "Running");
    } else {
        SetWindowText(openButton, TEXT("Not Running"));
        sprintf(msg, "Not Running");
    }
    if (strlen(Settings.directory) > 0) {
        char dirMsg[18+strlen(Settings.directory)];
        sprintf(dirMsg, "Currently Serving %s", Settings.directory);
        TextOut(hdc, 20, 225, TEXT(dirMsg), strlen(dirMsg));
    } else {
        char dirMsg[] = "Directory not chosen";
        TextOut(hdc, 20, 225, TEXT(dirMsg), strlen(dirMsg));
    }
    TextOut(hdc, 20, 20, TEXT(msg), strlen(msg));
    TextOut(hdc, 20, 135, TEXT("Port: "), strlen("Port: "));
    TextOut(hdc, 20, height-90, TEXT("C-server 1.0"), strlen("C-server 1.0"));
    EndPaint(hwnd, &ps);
    ReleaseDC(hwnd, hdc);
}

boolean onlyInts(char text[]) {
    int i=0;
    while (text[i]) {
        if (text[i] != '1' && text[i] != '2' && text[i] != '3' && text[i] != '4' && text[i] != '5' && text[i] != '6' && text[i] != '7' && text[i] != '8' && text[i] != '9' && text[i] != '0') {
            return FALSE;
        }
        i++;
    }
    return TRUE;
}

void createButtons(HWND hwnd) {
    hwndButton = CreateWindow("BUTTON", "Toggle",
        WS_TABSTOP | WS_VISIBLE | WS_CHILD,
        20, 45, 100, 40, hwnd, NULL, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);
    hwndChooseFolder = CreateWindow("BUTTON", "Choose Directory",
        WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_FLAT,
        20, 170, 125, 40, hwnd, NULL, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);
    corsSetting = CreateWindow("BUTTON", "Send CORS Headers",
        WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_AUTOCHECKBOX,
        20, 255, 175, 40, hwnd, NULL, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);
    indexSetting = CreateWindow("BUTTON", "Auto Render index.html",
        WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_AUTOCHECKBOX,
        20, 300, 225, 40, hwnd, NULL, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);
    if (Settings.index) {
        SendMessage(indexSetting, BM_SETCHECK, BST_CHECKED, 0);
    }
    if (Settings.cors) {
        SendMessage(corsSetting, BM_SETCHECK, BST_CHECKED, 0);
    }
    openButton = CreateWindow("BUTTON", "",
        WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_FLAT,
        20, 95, 300, 30, hwnd, NULL, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);
    openGithub = CreateWindow("BUTTON", "View on github",
        WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_FLAT,
        width-175, height-100, 125, 30, hwnd, NULL, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);
    char a[getIntTextLen(Settings.port)];
    memset(a, '\0', sizeof(a));
    sprintf(a, "%i", Settings.port);
    portInput = CreateWindow(TEXT("Edit"), TEXT(a), WS_CHILD | WS_VISIBLE | WS_BORDER, 60, 135, 140, 20, hwnd, NULL, NULL, NULL);
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam) {
    switch(msg)
    {
        case WM_COMMAND: {
            if ((int*)lParam == (int*)hwndButton) { //toggle running button pressed
                Settings.isRunning = !Settings.isRunning;
                toggleServer();
                PrintWindow(hwnd, NULL, 0);
            } else if ((int*)lParam == (int*)hwndChooseFolder) { //choose folder button pressed
                char fileName[sizeof(Settings.directory)];
                memset(fileName, '\0', sizeof(fileName));
                BROWSEINFO bInfo;
                bInfo.hwndOwner = NULL;
                bInfo.pidlRoot = NULL;
                bInfo.pszDisplayName = fileName;
                bInfo.lpszTitle = "Select Folder To Serve";
                bInfo.ulFlags = 0;
                bInfo.lpfn = NULL;
                bInfo.lParam = 0;
                bInfo.iImage = -1;
                LPITEMIDLIST lpItem = SHBrowseForFolder(&bInfo);
                if (lpItem != NULL) {
                    SHGetPathFromIDList(lpItem, fileName);
                    if (strlen(fileName) == 0) return 0;
                    memset(Settings.directory, '\0', sizeof(Settings.directory));
                    strcpy(Settings.directory, fileName);
                    int i=0;
                    while (Settings.directory[i] != '\0') {
                        if (Settings.directory[i] == '\\') {
                            Settings.directory[i] = '/';
                        }
                        i++;
                    }
                    PrintWindow(hwnd, NULL, 0);
                    saveSettings();
                }
            } else if ((int*)lParam == (int*)portInput) {
                if (wParam == 50331648) { //port input field changed
                    int len = GetWindowTextLength(portInput) + 1;
                    char text[len];
                    memset(text, '\0', sizeof(text));
                    GetWindowText(portInput, text, len);
                    boolean valid = onlyInts(text);
                    int a = atoi(text);
                    if (!valid) {
                        memset(text, '\0', sizeof(text));
                        sprintf(text, "%i", a);
                        SetWindowText(portInput, TEXT(text));
                    }
                    Settings.port = a;
                    saveSettings();
                }
            } else if ((int*)lParam == (int*)corsSetting) { //toggle CORS button pressed
                Settings.cors = (SendMessage(corsSetting, BM_GETCHECK, 0, 0) == BST_CHECKED);
                saveSettings();
            } else if ((int*)lParam == (int*)indexSetting) { //toggle auto render index button pressed
                Settings.index = (SendMessage(indexSetting, BM_GETCHECK, 0, 0) == BST_CHECKED);
                saveSettings();
            } else if ((int*)lParam == (int*)openButton) { //open button pressed
                if (Settings.error || !Settings.isRunning) return 0;
                char cmd[27+getIntTextLen(Settings.port)];
                sprintf(cmd, "start http://127.0.0.1:%i/", Settings.port);
                system(cmd);
            } else if ((int*)lParam == (int*)openGithub) { //open github button pressed
                system("start https://github.com/ethanaobrien/C-server");
            }
        }
        break;
        case WM_CLOSE:
            DestroyWindow(hwnd);
        break;
        case WM_DESTROY:
            PostQuitMessage(0);
        break;
        case WM_PAINT: {
            paintWindow(hwnd);
        }
        break;
        default:
            return DefWindowProc(hwnd, msg, wParam, lParam);
    }
    return 0;
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance,
    LPSTR lpCmdLine, int nCmdShow) {
    WNDCLASSEX wc;
    HWND hwnd;
    MSG Msg;
    wc.cbSize        = sizeof(WNDCLASSEX);
    wc.style         = 0;
    wc.lpfnWndProc   = WndProc;
    wc.cbClsExtra    = 0;
    wc.cbWndExtra    = 0;
    wc.hInstance     = hInstance;
    wc.hIcon         = LoadIcon(NULL, IDI_APPLICATION);
    wc.hCursor       = LoadCursor(NULL, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW+1);
    wc.lpszMenuName  = NULL;
    wc.lpszClassName = g_szClassName;
    wc.hIconSm       = LoadIcon(NULL, IDI_APPLICATION);

    if(!RegisterClassEx(&wc)) {
        MessageBox(NULL, "Window Registration Failed!", "Error!",
            MB_ICONEXCLAMATION | MB_OK);
        return 0;
    }
    
    hwnd = CreateWindowEx(
        WS_EX_CLIENTEDGE,
        g_szClassName,
        "Simple Server",
        WS_OVERLAPPEDWINDOW,
        (GetSystemMetrics(SM_CXSCREEN)/2)-(width/2), (GetSystemMetrics(SM_CYSCREEN)/2)-(height/2), width, height,
        NULL, NULL, hInstance, NULL);
        

    if(hwnd == NULL) {
        MessageBox(NULL, "Window Creation Failed!", "Error!",
            MB_ICONEXCLAMATION | MB_OK);
        return 0;
    }
    createButtons(hwnd);

    ShowWindow(hwnd, nCmdShow);
    UpdateWindow(hwnd);

    while(GetMessage(&Msg, NULL, 0, 0) > 0) {
        TranslateMessage(&Msg);
        DispatchMessage(&Msg);
    }
    return Msg.wParam;
}

void makeWindow() {
    WinMain(NULL, NULL, NULL, 1);
}
