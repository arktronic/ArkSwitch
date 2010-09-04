/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

// ArkSwitchNative.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "ArkSwitchNative.h"

#include <windowsx.h>

#include <iostream>
#include <fstream>
using namespace std;


struct ProcessEvents
{
	HWND FwdWindow;
	unsigned int EventType;
	unsigned int X;
	unsigned int Y;
};


static WNDPROC _oldProc = NULL;
static RECT _taskbarRect;
static ProcessEvents* _callback;
static BOOL _relocationMode = false;
static unsigned int _minX, _maxX;

BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		break;
	case DLL_THREAD_ATTACH:
		break;
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

LRESULT CALLBACK WindowProc (HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	
	HDC         hDC;
	HBRUSH      NewBrush;
	LRESULT		res;
	

	// If no callback is defined, we're wasting time. Get outta here.
	if(_callback == NULL)
		return CallWindowProc(_oldProc,hwnd,msg,wParam,lParam);

	switch(msg)
	{
		
		case WM_PAINT:
			if(_relocationMode) {
				res = CallWindowProc(_oldProc,hwnd,msg,wParam,lParam);
				hDC = GetWindowDC(hwnd);

				NewBrush = CreateSolidBrush(RGB(250, 175, 5));

				SelectObject(hDC, NewBrush);
				Rectangle(hDC, _minX, 0, _maxX, _taskbarRect.bottom);
				DeleteObject(NewBrush);

				ReleaseDC(hwnd, hDC);
				return res;
			}
			// else...
			return CallWindowProc(_oldProc,hwnd,msg,wParam,lParam);
		case WM_LBUTTONUP:
		case WM_LBUTTONDOWN:
		case WM_MOUSEMOVE:
			if(!_relocationMode) {
				if((unsigned int)GET_X_LPARAM(lParam) >= _minX && (unsigned int)GET_X_LPARAM(lParam) <= _maxX) {
					_callback->EventType = (msg == WM_LBUTTONUP ? 1 : (msg == WM_LBUTTONDOWN ? 2 : 3));
					_callback->X = GET_X_LPARAM(lParam);
					_callback->Y = GET_Y_LPARAM(lParam);
					PostMessage(_callback->FwdWindow, 0x404, 1, 1);

					return 0;
				}
			}
			return CallWindowProc(_oldProc,hwnd,msg,wParam,lParam);
			break;
		default:
			return CallWindowProc(_oldProc,hwnd,msg,wParam,lParam);
	}
	return 0;
}

ARKSWITCHNATIVE_API BOOL Register(ProcessEvents* callback)
{
	_callback = callback;

	HWND taskbar = NULL;

	if(_callback == NULL) {
		// Un-subclass the taskbar.
		if(_oldProc != NULL) SetWindowLong(FindWindow(L"HHTaskBar", NULL), GWL_WNDPROC, (LONG)_oldProc);
		_oldProc = NULL;
		return FALSE;
	}
	else {
		// Find the taskbar and get info about it.
		taskbar = FindWindow(L"HHTaskBar", NULL);
		if(taskbar == NULL) return FALSE;
		GetWindowRect(taskbar, &_taskbarRect);

		// Set values.
		if(callback->EventType == 255) {
			_minX = callback->X;
			_maxX = callback->Y;
		}
		else {
			_minX = 0;
			_maxX = (_taskbarRect.right - _taskbarRect.left) / 2;
		}
		
		// Subclass the taskbar.
		if(_oldProc == NULL) _oldProc = (WNDPROC)GetWindowLong(taskbar, GWL_WNDPROC);
		SetWindowLong(taskbar, GWL_WNDPROC, (LONG)WindowProc);
		InvalidateRect(taskbar, NULL, false);
		return TRUE;
	}
}

ARKSWITCHNATIVE_API BOOL SetActivationFieldRelocationMode(BOOL isOn){
	_relocationMode = isOn;
	HWND taskbar = FindWindow(L"HHTaskBar", NULL);
	if(taskbar == NULL)
	{
		_relocationMode = FALSE;
		return FALSE;
	}
	InvalidateRect(taskbar, NULL, true);
	return TRUE;
}
