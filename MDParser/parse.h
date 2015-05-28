
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <string>

#include "markdown.h"
#include "html.h"
#include "buffer.h"

#define READ_UNIT 1024
#define OUTPUT_UNIT 64

//std::string tohtml(const std::string &str);
//std::string tocss(const std::string &str);
//std::string tofinalhtml(const std::string &css, const std::string &content);
