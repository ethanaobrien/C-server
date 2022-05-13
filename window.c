#define NOMINMAX
#include <windows.h>
#include <winuser.h>

const char g_szClassName[] = "myWindowClass";

void toggleServer() {
    if (isRunning) {
        main_server = makeServer(DEFAULT_PORT, Settings);
    } else {
        pthread_cancel(main_server);
        closesocket(sock);
    }
}

void paintWindow(HWND hwnd) {
    PAINTSTRUCT ps;
    HDC hdc = BeginPaint(hwnd, &ps);
    char msg[isRunning?7:11];
    memset(msg, '\0', sizeof(msg));
    if (isRunning) {
        char portMsg[] = "open http://127.0.0.1:8887 in your browser";
        TextOut(hdc, 20, 100, TEXT(portMsg), strlen(portMsg));
        sprintf(msg, "Running");
    } else {
        sprintf(msg, "Not Running");
    }
    TextOut(hdc, 20, 20, TEXT(msg), strlen(msg));
    TextOut(hdc, 20, 130, TEXT("Port: "), strlen("Port: "));
    EndPaint(hwnd, &ps);
    ReleaseDC(hwnd, hdc);
}

HWND hwndButton, portInput;

void createButton(HWND hwnd) {
    hwndButton = CreateWindow("BUTTON", "toggle",
        WS_TABSTOP | WS_VISIBLE | WS_CHILD,
        20, 45, 100, 40, hwnd, NULL, (HINSTANCE)GetWindowLongPtr(hwnd, GWLP_HINSTANCE), NULL);
    portInput = CreateWindow(TEXT("Edit"), TEXT("8887"), WS_CHILD | WS_VISIBLE | WS_BORDER, 60, 130, 140, 20, hwnd, NULL, NULL, NULL); 
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam) {
    if (msg != 512 && msg != 32 && msg != 127 && msg != 132 && msg != 307) {
        //printf("%i\n", msg);
    }
    switch(msg)
    {
        case WM_COMMAND: {
            if ((int*)lParam == (int*)hwndButton) {
                isRunning = !isRunning;
                toggleServer();
                PrintWindow(hwnd, NULL, 0);
            } else if ((int*)lParam == (int*)portInput) {
                printf("%i\n", wParam);
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
