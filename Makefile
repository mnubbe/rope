CC = gmcs
SRCS = $(wildcard src/*.cs)
DIRS = $(subst /, ,$(CURDIR))
PROJ = $(word $(words $(DIRS)), $(DIRS))

APP = $(PROJ).exe
LDFLAGS = 
BINLIBS = -r:libs/opentk/OpenTK.dll -r:libs/opentk/OpenTK.Compatibility.dll -r:libs/opentk/OpenTK.GLControl.dll -r:System.Drawing -r:System.Windows.Forms


all: $(APP)

$(APP): $(SRCS)
	$(CC) -debug $(LDFLAGS) $(BINLIBS) $(SRCS) -out:$(APP) $(LIBS)

clean:
	rm -f $(APP) $(APP).mdb

