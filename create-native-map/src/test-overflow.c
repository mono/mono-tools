/* test the _cnm_return_val_if_overflow() macro */

#define DEBUG
/* #define DEBUG_DUMP */

#include "test.c"

#define CHECK(name,expected,to_t,val)             \
	int name (void)                                 \
	{                                               \
		_cnm_return_val_if_overflow (to_t, val, -1);  \
		return 0;                                     \
	}

#include "test-overflow.h"
#undef CHECK

struct test_info {
	int expected;
	const char *name;
	int (*test)(void);
} tests[] = {
#undef CHECK
#define CHECK(name,expected,to_t,val) { expected, #name, name },
#include "test-overflow.h"
#undef CHECK
};

int
contains (const char *name, int argc, char **argv)
{
	int i;
	if (argc == 1)
		return 1;
	for (i = 1; argv [i] != NULL; ++i) {
		if (!strcmp (name, argv [i]))
			return 1;
	}
	return 0;
}

int
main (int argc, char **argv)
{
	int i;
	int status = 0;

	for (i = 0; i < sizeof(tests)/sizeof(tests[0]); ++i) {
		struct test_info *t = &tests [i];
		int r;

		if (!contains (t->name, argc, argv))
			continue;
		r = t->test ();
		if (r != t->expected) {
			printf ("Test `%s' FAILED\n", t->name);
			status = i+1;
		}
	}
	return status;
}

