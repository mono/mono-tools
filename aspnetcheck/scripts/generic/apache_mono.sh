#!/bin/bash

#
# Generic checks for Mono configuration in Apache
#

#
# Checks for the type of Mono handler (if any) used in the
# virtual host. Echoes the type to standard output, if found, and returns 0. 
# If no Mono handler directive is found, echoes an empty string with no 
# newline and returns 1.
#
# Takes one parameter: path to the config file
#
# Exit codes:
#
#   0 - success
#   1 - failure
function apache_get_mono_handler ()
{
    local vhostpath="$1"
    
    if [ -z "$vhostpath" -o ! -f "$vhostpath" ]; then
	echo -n
	return 1
    fi

    sed -r -n -e 's/^[ \t]*[^#]*(SetHandler|AddHandler)[ \t]+mono.*/\1/pg' $vhostpath | tr -d ' \t\n'
    return 0
}

#
# Entry point for all vhost checks.
# Outputs only failures and warnings, returns 0 on success so that the caller
# can announce the success.
#
# Takes one parameter: path to the config file
#
# Exit codes:
#
#   0 - success
#   1 - failure (the 'failure' function has already been called)
#   2 - non-fatal notice shown
#   3 - fatal notice shown
function apache_check_mono_vhost ()
{
    local vhostpath="$1"
    local retval=0

    if [ -z "$vhostpath" ]; then
	failure Missing virtual host path parameter
	return 1
    fi

    if [ ! -f "$vhostpath" ]; then
	failure Virtual host configuration file $vhostpath does not exist
	return 1
    fi

    local old_indent="`get_indent`"
    local handler=`apache_get_mono_handler $vhostpath`

    set_indent "$old_indent  "

    case $handler in
	AddHandler) 
	    message
	    notice AddHandler is used. This is not recommended because of possible security issues.
	    retval=2 ;;
	*AddHandlerSetHandler*|*SetHandlerAddHandler*)
	    message
	    notice Both AddHandler and SetHandler are used. This is not supported.
	    retval=3 ;;
    esac
    
    if __apache_gather_applications "$vhostpath"; then
	__apache_gather_aliases "$vhostpath"
    else
	notice Mono application directives not found. Please read ${COLOR_BOLD}man mod_mono${COLOR_RESET} for more information.
	if [ $retval -lt 3 ]; then
	    retval=3
	fi
    fi

    set_indent "$old_indent"
    return $retval
}

function __apache_gather_applications ()
{
    local vhostpath="$1"
    APACHE_VHOST_APPLICATIONS=""
    declare -a APACHE_VHOST_APPLICATIONS

    local tmpfile=`mktemp "$TEMP_DIR/__apache_gather_aliases.XXXXXXXXXX"`
    egrep '^[ \t]*[^#]*(^|[ \t]+)(MonoApplications|AddMonoApplications|MonoApplicationsConfigFile|MonoApplicationsConfigDir)[ \t]+' $vhostpath > $tmpfile

    if [ $? -ne 0 ]; then
	return 1
    fi
#    __apache_gather_applications_from_input < $tmpfile
}

function __apache_gather_aliases ()
{
    local vhostpath="$1"
    APACHE_VHOST_ALIASES=""
    declare -a APACHE_VHOST_ALIASES

    local tmpfile=`mktemp "$TEMP_DIR/__apache_gather_aliases.XXXXXXXXXX"`
    egrep '^[ \t]*[^#]*(^|[ \t]+)Alias[ \t]+' "$vhostpath" > $tmpfile

    __apache_gather_aliases_from_input < $tmpfile
}

function __apache_gather_aliases_from_input ()
{
    local line
    while true; do
	read line
	if [ -z "$line" ]; then
	    break
	fi
	__apache_append_alias_to_array $line
    done
}

function __apache_append_alias_to_array ()
{
    if [ $# -ne 3 ]; then
	return
    fi

    #
    # $1 - Alias
    # $2 - /virtual/path
    # $3 - "/physical/path"
    #
    local ppath=`echo $3 | tr -d '"'`
    APACHE_VHOST_ALIASES[${#APACHE_VHOST_ALIASES[*]}]="$2:$ppath"
}
