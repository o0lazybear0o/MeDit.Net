#include "stdafx.h"
#include "parse.h"

/*
std::string tocss(const std::string &str)
{
    std::string ans = "<head><link href=\""
                + str
                + "\" rel=\"stylesheet\" type=\"text/css\"></head>";
    return ans;
}

std::string tofinalhtml(const std::string &css, const std::string &content)
{
    //std::string ans = "<!doctype html>\n<html>\n<head><link href=\""+ css + "\" rel=\"stylesheet\" type=\"text/css\"></head>\n<body>" + tohtml(content) + "</body>\n</html>";
    std::string ans = "<!doctype html>\n<html>\n<head></head>\n<body>" + tohtml(content) + "</body>\n <style>\n"+ css + "\n</style>\n</html>";
    return ans;
}
*/

// TODO: perform unicode<->utf8 conversion
extern "C" __declspec(dllexport) char* __stdcall ParseMarkdown(const char* str, const size_t len)
{
	struct buf *ib;
	static struct buf *ob = NULL;

    struct sd_callbacks callbacks;
    struct html_renderopt options;
    struct sd_markdown *markdown;

    /* reading everything */
    ib = bufnew(READ_UNIT);
    bufgrow(ib, READ_UNIT);

    size_t i = 0;
    for (i = 0; i < len; ++i)
    {
        *(ib->data + ib->size) = str[i];
        ib->size += 1;
        bufgrow(ib, ib->size + READ_UNIT);
    }

	if (!ob)
	{
		/* performing markdown parsing */
		ob = bufnew(OUTPUT_UNIT);
	}else
	{
		bufreset(ob);
	}
  
    sdhtml_renderer(&callbacks, &options, 0);
    markdown = sd_markdown_new(0, 16, &callbacks, &options);

    sd_markdown_render(ob, ib->data, ib->size, markdown);
    sd_markdown_free(markdown);

    /* cleanup */
    bufrelease(ib);
    // bufrelease(ob);

	// append NULL-terminator
	bufgrow(ob, ob->size+1);
	ob->data[ob->size] = 0;
	return (char*)ob->data;
}