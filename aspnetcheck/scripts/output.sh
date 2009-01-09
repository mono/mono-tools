#
# Output functions for aspnetcheck
#

COLOR_RESET="\e[0m"
COLOR_BOLD="\e[1m"
COLOR_RED="\e[31m"
COLOR_GREEN="\e[32m"
COLOR_BLUE="\e[34m"
COLOR_WHITE="\e[37m"
COLOR_CYAN="\e[36m"
COLOR_YELLOW="${COLOR_BOLD}\e[33m"

ERASE_ENTIRE_LINE="\r\e[2K\r"

MESSAGE_INDENT=""

function message ()
{
    echo -n -e "$MESSAGE_INDENT"
    echo -e $*
}

function __erase_line ()
{
    message -n "${ERASE_ENTIRE_LINE}"
}

function die ()
{
    EXIT_STATUS=1
    message $COLOR_BOLD$COLOR_RED$*$COLOR_RESET
    exit $EXIT_STATUS
}

function failure ()
{
    EXIT_STATUS=1
    __erase_line
    message "[ ${COLOR_RED}fail${COLOR_RESET} ]  $*"
}

function success ()
{
    __erase_line
    message "[ ${COLOR_GREEN}ok${COLOR_RESET} ]  $*"
}

function warning ()
{
    __erase_line
    message "[ ${COLOR_YELLOW}warn${COLOR_RESET} ]  $*"
}

function notice ()
{
    __erase_line
    message "[ ${COLOR_CYAN}note${COLOR_RESET} ]  $*"
}

function checking ()
{
    __erase_line
    message -n "[ ${COLOR_BOLD}${COLOR_BLUE}checking${COLOR_RESET} ]  $*"
}

function section_banner ()
{
    __erase_line
    message "[ ${COLOR_BOLD}${COLOR_WHITE}$*${COLOR_RESET} ]"
}

function on_exit ()
{
    __remove_temp_directory
    if [ $EXIT_STATUS -eq 0 ]; then
	message ${COLOR_GREEN}Done.${COLOR_RESET}
    else
	message ${COLOR_BOLD}${COLOR_RED}Done.${COLOR_RESET}
    fi
}

function init_log ()
{
    local dir="`dirname $LOG_LOCATION`"
    if [ ! -d "$dir" ]; then
	install -d -m 700 "$dir"
    fi

    echo Log started: `date` > "$LOG_LOCATION"
    message "Logging check messages to: ${COLOR_BOLD}${COLOR_WHITE}$LOG_LOCATION${COLOR_RESET}"
}

function logwrite ()
{
    echo -e "\n[ $1 ]" >> "$LOG_LOCATION"
    shift
    echo "$*" >> "$LOG_LOCATION"
}

function set_indent ()
{
    MESSAGE_INDENT="$1"
}

function get_indent ()
{
    echo "$MESSAGE_INDENT"
}

function reset_indent ()
{
    MESSAGE_INDENT=""
}
