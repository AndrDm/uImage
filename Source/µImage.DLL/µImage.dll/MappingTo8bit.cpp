// Mapping16to8bit.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

extern "C"
{
	__declspec(dllexport) void __cdecl  Mapping16to8(
		unsigned short *src_ptr_16bit, unsigned char *dst_ptr_8bit,
		int src_bytesPerLine, int dst_bytesPerLine,
		int width, int height, int min, int max,
		int mode)
    {
       	int x, y, i;
       	unsigned short imin, imax;
       	unsigned short *tmp_ptr_16bit;
       	unsigned char LUT16[65536];
       
    	if (!src_ptr_16bit || !dst_ptr_8bit) return;
    
    	switch (mode){
    		default:
    		case 0:
    		case 1:
    			imin = 65535; imax = 0;
    			tmp_ptr_16bit = src_ptr_16bit;
    			for (y = 0; y < height; y++) {
    				for (x = 0; x < width; x++) {
    					if ((*tmp_ptr_16bit) > imax) imax = (*tmp_ptr_16bit);
    					if ((*tmp_ptr_16bit) < imin) imin = (*tmp_ptr_16bit);
    					tmp_ptr_16bit++;
    				}
    				tmp_ptr_16bit += (src_bytesPerLine >> 1) - width;
    			}
    			break;
    		case 2:
    			imax = max; imin = min;
    			break;
    	}
    	imax = imax > 65535 ? 65535 : imax;
    	for (i = 0; i < imin; i++) LUT16[i] = 0;
    	for (i = imin; i < imax; i++) LUT16[i] = (int)(double(i - imin) / (imax - imin) * 255.0);
    	for (i = imax; i < 65536; i++) LUT16[i] = 255;
    
    	for (y = 0; y < height; y++) {
    		for (x = 0; x < width; x++) {
    			*dst_ptr_8bit = LUT16[*src_ptr_16bit];
    			src_ptr_16bit++;
    			dst_ptr_8bit++;
    		}
    		src_ptr_16bit += (src_bytesPerLine >> 1) - width;
    		dst_ptr_8bit += dst_bytesPerLine - width;
    	}
    }
    
    __declspec(dllexport) void __cdecl  Mapping8to8(unsigned char *src_ptr_8bit, unsigned char *dst_ptr_8bit,
    	int src_bytesPerLine, int dst_bytesPerLine,
    	int width, int height, int min, int max,
    	int mode)
    {
    	int x, y, i;
    	int imin, imax;
    	unsigned char *tmp_ptr_8bit;
    
    	unsigned char LUT8[256];
    
    	if (!src_ptr_8bit || !dst_ptr_8bit) return;
    
    	switch (mode) {
    	default:
    	case 0:
    		imin = 0; imax = 255;
    		break;
    	case 1:
    		imin = 255; imax = 0;
    		tmp_ptr_8bit = src_ptr_8bit;
    		for (y = 0; y < height; y++) {
    			for (x = 0; x < width; x++) {
    				if ((*tmp_ptr_8bit) > imax) imax = (*tmp_ptr_8bit);
    				if ((*tmp_ptr_8bit) < imin) imin = (*tmp_ptr_8bit);
    				tmp_ptr_8bit++;
    			}
    			tmp_ptr_8bit += (src_bytesPerLine) - width;
    		}
    		break;
    	case 2:
    		imax = max; imin = min;
    		break;
    	}
    
    	imax = imax > 255 ? 255 : imax;
    	for (i = 0; i < imin; i++) LUT8[i] = 0;
    	for (i = imin; i < imax; i++) LUT8[i] = (int) (double(i - imin) / (imax - imin) * 255.0);
    	for (i = imax; i < 256; i++) LUT8[i] = 255;
    
    	for (y = 0; y < height; y++) {
    		for (x = 0; x < width; x++) {
    			*dst_ptr_8bit = LUT8[*src_ptr_8bit];
    			src_ptr_8bit++;
    			dst_ptr_8bit++;
    		}
    		src_ptr_8bit += src_bytesPerLine - width;
    		dst_ptr_8bit += dst_bytesPerLine - width;
    	}
    }
}