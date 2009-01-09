#
# Checks loader functions
#
function __load_from ()
{
    if [ ! -d "$1" ]; then
	return
    fi

    for f in "$1"/*.check; do
	if [ ! -x "$f" ]; then
	    continue
	fi

	source "$f"
    done
}

function load_checks ()
{
    if [ ! -d "$SCRIPTS_DIR/checks/generic" ]; then
	die Generic checks directory not found
    fi

    CHECKS_LIST=""
    #
    # First the generic checks
    #
    __load_from "$SCRIPTS_DIR/checks/generic"

    #
    # Now the vendor ones
    #
    __load_from "$SCRIPTS_DIR/checks/$OS_VENDOR"

    #
    # The vendor OS ones
    #
    __load_from "$SCRIPTS_DIR/checks/$OS_VENDOR/$OS_NAME"

    #
    # And the OS version ones
    #
    __load_from "$SCRIPTS_DIR/checks/$OS_VENDOR/$OS_NAME/$OS_RELEASE"

    #
    # Find and sort in alphabetical order all the check functions defined by
    # the loaded scripts
    #
    for ch in `declare -pF | cut -d ' ' -f 3 | sort -d -s`; do
	case $ch in
	    check_*) CHECKS_LIST="$CHECKS_LIST $ch" ;;
	esac
    done
}

readonly -f __load_from load_checks

