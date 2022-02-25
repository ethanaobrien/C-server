#include <winsock2.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <dirent.h>
#include "handler.c"

#define DEFAULT_PORT 8887

int main(int argc, char ** argv) {

  int addr_len;
  struct sockaddr_in local, client_addr;

  SOCKET sock, msg_sock;
  WSADATA wsaData;

  if (WSAStartup(0x202, & wsaData) == SOCKET_ERROR) {
    // stderr: standard error are printed to the screen.
    fprintf(stderr, "WSAStartup failed with error %d\n", WSAGetLastError());
    //WSACleanup function terminates use of the Windows Sockets DLL. 
    WSACleanup();
    return -1;
  }
  // Fill in the address structure
  local.sin_family = AF_INET;
  local.sin_addr.s_addr = INADDR_ANY;
  local.sin_port = htons(DEFAULT_PORT);

  sock = socket(AF_INET, SOCK_STREAM, 0); //TCp socket

  if (sock == INVALID_SOCKET) {
    fprintf(stderr, "socket() failed with error %d\n", WSAGetLastError());
    WSACleanup();
    return -1;
  }

  if (bind(sock, (struct sockaddr * ) & local, sizeof(local)) == SOCKET_ERROR) {
    fprintf(stderr, "bind() failed with error %d\n", WSAGetLastError());
    WSACleanup();
    return -1;
  }

  if (listen(sock, 5) == SOCKET_ERROR) {
    fprintf(stderr, "listen() failed with error %d\n", WSAGetLastError());
    WSACleanup();
    return -1;
  }

  printf("Listening on 127.0.0.1%i\n", DEFAULT_PORT);

  while (1) {
    addr_len = sizeof(client_addr);
    msg_sock = accept(sock, (struct sockaddr * ) & client_addr, & addr_len);
    if (msg_sock == INVALID_SOCKET) {
      fprintf(stderr, "accept() failed with error %d\n", WSAGetLastError());
      WSACleanup();
      return -1;
    }

    if (msg_sock == -1) {
      perror("Unable to accept connection.");
      continue;
    }

    //printf("Connection from %s:%d\n", inet_ntoa(client_addr.sin_addr), htons(client_addr.sin_port));
    onRequest(msg_sock);
    closesocket(msg_sock);
  }
  WSACleanup();
}