#Modified from the NU EECS Magic Makefile
#
# If you put this in a directory with C# code, then
#
#   make        -- compiles your project into program.exe
#   make clean  -- removes compiled item
#   make handin -- creates a project Zip file for hand in (commented out)
#
# All .cpp flles are included.
# UnitTest++ is included.
# Changes to any header file or this Makefile triggers recompilation
# of all .cpp files.

CC = mcs
SRCS = $(wildcard src/*.cs)
#No headers in csharp
#HDRS = $(wildcard *.h)
#not sure if this translates to C#
#OBJS = $(SRCS:.cpp=.o)
DIRS = $(subst /, ,$(CURDIR))
PROJ = $(word $(words $(DIRS)), $(DIRS))

APP = $(PROJ).exe
CFLAGS = -c -g -Wall -I/usr/local/include
LDFLAGS = 
LIBS = 

# gcc 4 in Cygwin needs non-standard --enable-auto-import option
#ifneq (,$(findstring CYGWIN,$(shell uname)))
#  LDFLAGS += -Wl,--enable-auto-import
#endif

all: $(APP)

$(APP):
#	$(CC) $(LDFLAGS) $(SRCS) -out:$(APP) $(LIBS)
	$(CC) $(LDFLAGS) $(SRCS) -out:$(APP) $(LIBS)
#$(APP): $(OBJS)
#	$(CC) $(LDFLAGS) $(OBJS) -o $(APP) $(LIBS)

clean:
	rm -f $(APP)

#Could be used for prepping for a commit potentially
#handin:
#	rm -f $(PROJ).zip
#	zip $(PROJ).zip -q Makefile *.cs
#	unzip -l $(PROJ).zip
