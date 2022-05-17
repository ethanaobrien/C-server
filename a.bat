gcc in.c -lws2_32 -lgdi32 -lcomdlg32 -lole32 -mwindows 


to debug:

gcc -g in.c -lws2_32 -lgdi32 -lcomdlg32 -lole32 -mwindows

gdb a

then enter "r"
