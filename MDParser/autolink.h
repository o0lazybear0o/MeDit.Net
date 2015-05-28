
#ifndef UPSKIRT_AUTOLINK_H
#define UPSKIRT_AUTOLINK_H

#include "buffer.h"

#ifdef __cplusplus
extern "C" {
#endif

enum {
	SD_AUTOLINK_SHORT_DOMAINS = (1 << 0),
};

int
sd_autolink_issafe(const uint8_t *link, size_t link_len);

size_t
sd_autolink__www(size_t *rewind_p, struct buf *link,
	uint8_t *data, size_t offset, size_t size, unsigned int flags);

size_t
sd_autolink__email(size_t *rewind_p, struct buf *link,
	uint8_t *data, size_t offset, size_t size, unsigned int flags);

size_t
sd_autolink__url(size_t *rewind_p, struct buf *link,
	uint8_t *data, size_t offset, size_t size, unsigned int flags);

#ifdef __cplusplus
}
#endif

#endif

/* vim: set filetype=c: */
