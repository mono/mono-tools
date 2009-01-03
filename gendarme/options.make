# note that we generate mdb files even in release
COMMON_OPTIONS := -debug+ -d:TRACE $(if $(DEBUG),-d:DEBUG -checked+,-optimize+)
COMMON_OPTIONS += -nowarn:1591	# Missing XML comment

GENDARME_OPTIONS := $(COMMON_OPTIONS) -warn:4 -warnaserror+
TESTS_OPTIONS := $(COMMON_OPTIONS) -warn:0
