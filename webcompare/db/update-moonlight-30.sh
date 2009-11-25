#!/bin/bash

H=~mono-web/go-mono/status/db
cd ${H} || exit 1
ATEXIT_RUN=0
trap atexit EXIT INT QUIT

function atexit () {
	test $ATEXIT_RUN -ne 0 && return
	ATEXIT_RUN=1
	rm -rf ${DLDIR}
	rm -f ${LOCK_FILE}
}

LOG_FILE="${H}/../binary/moonlight3.log"
LOCK_FILE="${H}/../binary/moonlight3.lock"
DLDIR="${H}/temp-moonlight3"

lockfile -l 120 ${LOCK_FILE}
if [ $? -ne 0 ] ; then
	echo "lockfile timed out" >> ${LOG_FILE}
	exit 1
fi

rm -rf ${DLDIR} || exit 1
mkdir -p ${DLDIR} || exit 1
cd ${DLDIR} || exit 1
wget --timeout=60 -a ${LOG_FILE} -O ${DLDIR}/xpi.zip.gz 'http://moon.sublimeintervention.com/DownloadLatestFile.aspx?lane=moon-trunk-2.0&filename=novell-moonlight.xpi'
if [ $? -ne 0 ] ; then
	echo "Error downloading XPI" >> ${LOG_FILE}
	exit 1
fi
gunzip xpi.zip.gz && unzip -qq xpi.zip >> ${LOG_FILE} 2>&1 
test $? -eq 0 || exit 1
cp ${DLDIR}/plugins/moonlight/*.dll ${H}/../binary/2.1/ >> ${LOG_FILE} 2>&1 
test $? -eq 0 || exit 1
cd ${H}
date >> ${LOG_FILE}
nice -20 mono webcompare-db.exe 'SL3 2.1' >> ${LOG_FILE} 2>&1
date >> ${LOG_FILE}
exit 0

