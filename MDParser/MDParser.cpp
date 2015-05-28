// MDParser.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "MDParser.h"


// This is an example of an exported function.
MDPARSER_API int fnMDParser(void)
{
	return 42;
}

// This is the constructor of a class that has been exported.
// see MDParser.h for the class definition
CMDParser::CMDParser()
{
	return;
}
