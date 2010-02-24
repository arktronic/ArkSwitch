/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the ARKSWITCHNATIVE_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// ARKSWITCHNATIVE_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef ARKSWITCHNATIVE_EXPORTS
#define ARKSWITCHNATIVE_API extern "C" __declspec(dllexport)
#else
#define ARKSWITCHNATIVE_API __declspec(dllimport)
#endif

BOOL CALLBACK EnumWindowsProc(HWND hwnd, LPARAM lParam);
LRESULT CALLBACK WindowProc (HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);