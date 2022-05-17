#define NOMINMAX
#include <windows.h>
#include <winuser.h>
#include <commdlg.h>
#include <ShlObj.h>

const char g_szClassName[] = "myWindowClass";

void toggleServer() {
    if (Settings.isRunning) {
        main_server = makeServer(Settings.port, Settings);
    } else {
        pthread_cancel(main_server);
        closesocket(sock);
    }
}

void paintWindow(HWND hwnd) {
    PAINTSTRUCT ps;
    HDC hdc = BeginPaint(hwnd, &ps);
    char msg[Settings.error?5:(Settings.isRunning?7:11)];
    memset(msg, '\0', sizeof(msg));
    if (Settings.error) {
        sprintf(msg, "Error");
        char portMsg[41+getIntTextLen(Settings.port)];
        sprintf(portMsg, "There was an error listening on the port %i", Settings.port);
        TextOut(hdc, 20, 100, TEXT(portMsg), strlen(portMsg));
    } else if (Settings.isRunning) {
        char portMsg[38+getIntTextLen(Settings.port)];
        sprintf(portMsg, "Open http://127.0.0.1:%i in your browser", Settings.port);
        TextOut(hdc, 20, 100, TEXT(portMsg), strlen(portMsg));
        sprintf(msg, "Running");
    } else {
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
    EndPaint(hwnd, &ps);
    ReleaseDC(hwnd, hdc);
}

HWND hwndButton, portInput, hwndChooseFolder;

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

void createButton(HWND hwnd) {
    hwndButton = CreateWindow("BUTTON", "Toggle",
        WS_TABSTOP | WS_VISIBLE | WS_CHILD,
        20, 45, 100, 40, hwnd, NULL, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);
    hwndChooseFolder = CreateWindow("BUTTON", "Choose Directory",
        WS_TABSTOP | WS_VISIBLE | WS_CHILD,
        20, 170, 125, 40, hwnd, NULL, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);
    char a[getIntTextLen(Settings.port)];
    memset(a, '\0', sizeof(a));
    sprintf(a, "%i", Settings.port);
    portInput = CreateWindow(TEXT("Edit"), TEXT(a), WS_CHILD | WS_VISIBLE | WS_BORDER, 60, 130, 140, 20, hwnd, NULL, NULL, NULL); 
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam) {
    switch(msg)
    {
        case WM_COMMAND: {
            if ((int*)lParam == (int*)hwndButton) {
                Settings.isRunning = !Settings.isRunning;
                toggleServer();
                PrintWindow(hwnd, NULL, 0);
            } else if ((int*)lParam == (int*)hwndChooseFolder) {
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
                    PrintWindow(hwnd, NULL, 0);
                    saveSettings();
                }
            } else if ((int*)lParam == (int*)portInput) {
                if (wParam == 50331648) {
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
    int width = 410, height = 700;
    
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
    createButton(hwnd);

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
