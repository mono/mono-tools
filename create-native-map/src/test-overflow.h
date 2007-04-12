/* CHECK ( fuction-name, type, value, expected-return-value ) */

/* 
 * function-name pattern:
 *  <STATUS> <TYPE> <VALUE>
 * where
 *  STATUS: p=pass, f=fail
 *    TYPE: b=byte, c=sbyte, i=int, I=uint, s=short, S=ushort, 
 *          l=long64, L=ulong64
 *   VALUE: mn=min, mx=max, mnm1=max-1, mxp1=max+1
 */

CHECK(pc1,     0, gint8,  1)
CHECK(pc0,     0, gint8,  0)
CHECK(pcn1,    0, gint8, -1)
CHECK(pcmn,    0, gint8, G_MININT8)
CHECK(pcmx,    0, gint8, G_MAXINT8)
CHECK(fcmnm1, -1, gint8, G_MININT8-1)
CHECK(fcmxp1, -1, gint8, G_MAXINT8+1)

CHECK(pb1,     0, guint8, 1)
CHECK(pb0,     0, guint8, 0)
CHECK(pbmx,    0, guint8, G_MAXUINT8)
CHECK(fbmnm1, -1, guint8, -1)
CHECK(fbmxp1, -1, guint8, G_MAXUINT8+1)

CHECK(ps1,     0, gint16,  1)
CHECK(ps0,     0, gint16,  0)
CHECK(psn1,    0, gint16, -1)
CHECK(psmn,    0, gint16, G_MININT16)
CHECK(psmx,    0, gint16, G_MAXINT16)
CHECK(fsmnm1, -1, gint16, G_MININT16-1)
CHECK(fsmxp1, -1, gint16, G_MAXINT16+1)

CHECK(pS1,     0, guint16, 1)
CHECK(pS0,     0, guint16, 0)
CHECK(pSmx,    0, guint16, G_MAXUINT16)
CHECK(fSmnm1, -1, guint16, -1)
CHECK(fSmxp1, -1, guint16, G_MAXUINT16+1)

CHECK(pi1,     0, gint32,  1)
CHECK(pi0,     0, gint32,  0)
CHECK(pin1,    0, gint32, -1)
CHECK(pimn,    0, gint32, G_MININT32)
CHECK(pimx,    0, gint32, G_MAXINT32)
CHECK(fimnm1, -1, gint32, ((gint64) G_MININT32-1))
CHECK(fimxp1, -1, gint32, ((gint64) G_MAXINT32+1))

CHECK(pI1,     0, guint32, 1)
CHECK(pI0,     0, guint32, 0)
CHECK(pImx,    0, guint32, G_MAXUINT32)
CHECK(fImnm1, -1, guint32, -1)
CHECK(fImxp1, -1, guint32, G_MAXUINT32+1)

CHECK(pl1,     0, gint64,  1)
CHECK(pl0,     0, gint64,  0)
CHECK(pln1,    0, gint64, -1)
CHECK(plmn,    0, gint64, G_MININT64)
CHECK(plmx,    0, gint64, G_MAXINT64)
/* CHECK(flmnm1, gint64, G_MININT64-1, -1) */
CHECK(flmxp1, -1, gint64, ((guint64) G_MAXINT64+1))

CHECK(pL1,     0, guint64, 1)
CHECK(pL0,     0, guint64, 0)
CHECK(pLmx,    0, guint64, G_MAXUINT64)
CHECK(fLmnm1, -1, guint64, -1)
/* CHECK(fLmxp1, guint64, G_MAXUINT64+1, -1) */

