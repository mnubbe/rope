CC = mcs
SRCS = $(wildcard src/*.cs)
DIRS = $(subst /, ,$(CURDIR))
PROJ = $(word $(words $(DIRS)), $(DIRS))

APP = $(PROJ).exe
CFLAGS = -c -g -Wall -I/usr/local/include
LDFLAGS = -pkg:mono-cairo -pkg:gtk-sharp-2.0


all: $(APP)

$(APP):
	$(CC) $(LDFLAGS) $(SRCS) -out:$(APP) $(LIBS)

clean:
	rm -f $(APP)

