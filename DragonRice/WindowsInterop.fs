module DragonRice.WindowsInterop

open System
open System.Runtime.InteropServices
open System.Text

type WindowHandle =
    struct
        val RawPtr: IntPtr
        new(rawPtr: IntPtr) = { RawPtr = rawPtr }
    end

    static member invalid =
        new WindowHandle(IntPtr.Zero)

    member self.isValid =
        self.RawPtr <> IntPtr.Zero

module NativeMethods =
    let EnumWindows_ContinueEnumerating = true
    let EnumWindows_StopEnumerating = false

    type EnumWindowsProc = delegate of IntPtr * IntPtr -> bool (* hWnd, lParam *)

    // Get window

    [<DllImport("user32.dll")>]
    extern bool EnumWindows (EnumWindowsProc enumProc, IntPtr lParam)

    [<DllImport("user32.dll")>]
    extern bool EnumDesktopWindows (IntPtr hDesktop, EnumWindowsProc enumProc, IntPtr lParam)

    [<DllImport("user32.dll")>]
    extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount)

    // Window visibility

    [<DllImport("user32.dll")>]
    extern bool IsWindowVisible(IntPtr hWnd)

    let SW_HIDE = 0;
    let SW_SHOW = 5;

    [<DllImport("user32.dll")>]
    extern bool ShowWindow(IntPtr hWnd, int nCmdShow)

    // Window foreground

    [<DllImport("user32.dll")>]
    extern IntPtr GetForegroundWindow()

    [<DllImport("user32.dll")>]
    extern bool SetForegroundWindow(IntPtr hWnd)

    // Window size

    let SWP_HIDE = 0x0080u;
    let SWP_SHOW = 0x0040u;

    [<DllImport("user32.dll")>]
    extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndA, int X, int Y, int Cx, int Cy, uint flags)

module WrapperMethods =
    // Slightly more rubust way to get the task bar
    let findWindow predicate =
        let mutable found = WindowHandle.invalid
        let predicateDelegate = 
            NativeMethods.EnumWindowsProc(fun wnd param -> 
                let window = new WindowHandle(wnd)
                if predicate window then
                    found <- window
                    NativeMethods.EnumWindows_StopEnumerating
                else
                    NativeMethods.EnumWindows_ContinueEnumerating)

        let _ = NativeMethods.EnumWindows(predicateDelegate, IntPtr.Zero)

        if found.isValid then
            Some found
        else
            None

    let getAllDesktopWindows () =
        let mutable list = []
        let predicateDelegate = 
            NativeMethods.EnumWindowsProc(fun wnd param -> 
                let window = new WindowHandle(wnd)
                list <- window::list
                NativeMethods.EnumWindows_ContinueEnumerating)

        let _ = NativeMethods.EnumDesktopWindows(IntPtr.Zero, predicateDelegate, IntPtr.Zero)

        list

    let getWindowClassName (handle: WindowHandle) =
        let builder = new StringBuilder(255);
        let _ = NativeMethods.GetClassName(handle.RawPtr, builder, builder.Capacity)
        builder.ToString()

    let hideWindow (handle: WindowHandle) = NativeMethods.ShowWindow(handle.RawPtr, NativeMethods.SW_HIDE)
    let showWindow (handle: WindowHandle) = NativeMethods.ShowWindow(handle.RawPtr, NativeMethods.SW_SHOW)

    let toggleWindowVisibility (handle: WindowHandle) =
        match NativeMethods.IsWindowVisible handle.RawPtr with
        | true -> hideWindow handle
        | false -> showWindow handle

    let trueHideWindow (handle: WindowHandle) = NativeMethods.SetWindowPos(handle.RawPtr, IntPtr.Zero, 0, 0, 0, 0, NativeMethods.SWP_HIDE)
    
        
