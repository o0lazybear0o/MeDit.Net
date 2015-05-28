// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the MDPARSER_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// MDPARSER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef MDPARSER_EXPORTS
#define MDPARSER_API __declspec(dllexport)
#else
#define MDPARSER_API __declspec(dllimport)
#endif

// This class is exported from the MDParser.dll
class MDPARSER_API CMDParser {
public:
	CMDParser(void);
	// TODO: add your methods here.
};

extern MDPARSER_API int nMDParser;

MDPARSER_API int fnMDParser(void);
