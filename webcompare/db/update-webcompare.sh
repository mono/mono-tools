#!/bin/bash

cd ~mono-web/go-mono/status/db || exit 1
ATEXIT_RUN=0
TOKEN_FILE="../binary/2.0/mscorlib.dll"
STAMP_FILE="../binary/webdb.stamp"
LOG_FILE="../binary/webdb.log"
LOCK_FILE="../binary/webdb.lock"

function atexit () {
	test $ATEXIT_RUN -ne 0 && return
	ATEXIT_RUN=1
	rm -f ${LOCK_FILE}
}

lockfile -l 120 ${LOCK_FILE}
trap atexit EXIT INT QUIT
if [ $? -ne 0 ] ; then
	echo "lockfile timed out" >> ${LOG_FILE}
	exit 1
fi

if [ ! -f ${STAMP_FILE} -o ${TOKEN_FILE} -nt ${STAMP_FILE} ] ; then
	touch -r ${TOKEN_FILE} ${STAMP_FILE}
	date > ${LOG_FILE}
	nice -20 mono webcompare-db.exe '3.5 2.0' '2.0 2.0' '4.0 4.0' >> ${LOG_FILE} 2>&1
	date >> ${LOG_FILE}
else
	D=$(date)
	echo "${D}: nothing to update" > ${LOG_FILE}
fi
rm -f ${LOCK_FILE}
exit 0

