MODAPI = 2
ROOT_DIR := $(shell dirname "$(realpath $(lastword $(MAKEFILE_LIST)))")

OUTPUT = "$(ROOT_DIR)/private/BassBoom.Cli/bin/$(ENVIRONMENT)/net8.0"
BINARIES = "$(ROOT_DIR)/assets/bassboom"
MANUALS = "$(ROOT_DIR)/assets/bassboom.1"
DESKTOPS = "$(ROOT_DIR)/assets/bassboom.desktop"
BRANDINGS = "$(ROOT_DIR)/assets/OfficialAppIcon-BassBoom-512.png"

ARCH := $(shell if [ `uname -m` = "x86_64" ]; then echo "linux-x64"; else echo "linux-arm64"; fi)

ifndef DESTDIR
FDESTDIR := /usr/local
else
FDESTDIR := $(DESTDIR)/usr
endif

ifndef ENVIRONMENT
ENVIRONMENT := Release
endif

.PHONY: all install lite

# General use

all:
	$(MAKE) all-online BUILDARGS="$(BUILDARGS)"

all-online:
	$(MAKE) invoke-build ENVIRONMENT=Release BUILDARGS="$(BUILDARGS)"

dbg:
	$(MAKE) invoke-build ENVIRONMENT=Debug BUILDARGS="$(BUILDARGS)"

dbg-ci:
	$(MAKE) invoke-build ENVIRONMENT=Debug BUILDARGS="-p:ContinuousIntegrationBuild=true $(BUILDARGS)"

rel-ci:
	$(MAKE) invoke-build ENVIRONMENT=Release BUILDARGS="-p:ContinuousIntegrationBuild=true $(BUILDARGS)"

doc: invoke-doc-build

clean:
	bash tools/clean.sh

all-offline:
	$(MAKE) invoke-build-offline BUILDARGS="-p:ContinuousIntegrationBuild=true $(BUILDARGS)"

init-offline:
	$(MAKE) invoke-init-offline

install:
	mkdir -m 755 -p $(FDESTDIR)/bin $(FDESTDIR)/lib/bassboom-$(MODAPI) $(FDESTDIR)/share/applications $(FDESTDIR)/share/man/man1/
	install -m 755 -t $(FDESTDIR)/bin/ $(BINARIES)
	install -m 644 -t $(FDESTDIR)/share/man/man1/ $(MANUALS)
	find "$(OUTPUT)" -mindepth 1 -type d -exec sh -c 'mkdir -p -m 755 "$(FDESTDIR)/lib/bassboom-$(MODAPI)/$$(realpath --relative-to "$(OUTPUT)" "$$0")"' {} \;
	find "$(OUTPUT)" -mindepth 1 -type f -exec sh -c 'install -m 644 -t "$(FDESTDIR)/lib/bassboom-$(MODAPI)/$$(dirname $$(realpath --relative-to "$(OUTPUT)" "$$0"))" "$$0"' {} \;
	install -m 755 -t $(FDESTDIR)/share/applications/ $(DESKTOPS)
	install -m 755 -t $(FDESTDIR)/lib/bassboom-$(MODAPI)/ $(BRANDINGS)
	mv $(FDESTDIR)/bin/bassboom $(FDESTDIR)/bin/bassboom-$(MODAPI)
	mv $(FDESTDIR)/share/man/man1/bassboom.1 $(FDESTDIR)/share/man/man1/bassboom-$(MODAPI).1
	mv $(FDESTDIR)/share/applications/bassboom.desktop $(FDESTDIR)/share/applications/bassboom-$(MODAPI).desktop
	sed -i 's|/usr/lib/bassboom|/usr/lib/bassboom-$(MODAPI)|g' $(FDESTDIR)/bin/bassboom-*
	sed -i 's|/usr/lib/bassboom|/usr/lib/bassboom-$(MODAPI)|g' $(FDESTDIR)/share/applications/bassboom-$(MODAPI).desktop
	sed -i 's|/usr/bin/bassboom|/usr/bin/bassboom-$(MODAPI)|g' $(FDESTDIR)/share/applications/bassboom-$(MODAPI).desktop
	find '$(FDESTDIR)/lib/' -type d -name "runtimes" -exec sh -c 'find $$0 -mindepth 1 -maxdepth 1 -not -name $(ARCH) -type d -exec rm -rf \{\} \;' {} \;

# Below targets specify functions for full build

invoke-build:
	bash tools/build.sh "$(ENVIRONMENT)" $(BUILDARGS)
    
invoke-doc-build:
	bash tools/docgen.sh

invoke-build-offline:
	HOME=`pwd`"/debian/homedir" bash tools/build.sh Release $(BUILDARGS)

invoke-init-offline:
	bash tools/localize.sh
