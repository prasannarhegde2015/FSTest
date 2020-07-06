'****************Script to Register dll for WellFlo UI on any machine *******************************
Const UNREGISTER_COMMAND = "%REGASM% /verbose /unregister %FILENAME%"
Const REGISTER_COMMAND = "%REGASM% /verbose /codebase %FILENAME% /tlb:%TLBFILENAME%"
Const DEBUG_MODE = False
Dim regAsm
'*************  Fucntion to Register the Utility *************************************************
Function FindRegASM()
    Set FSO = CreateObject("Scripting.FileSystemObject")
    regAsm = ""
    sFramework = FSO.GetSpecialFolder(0) & "\Microsoft.NET\Framework\"
    NETVersions = Array("v3.5", "v2.0.50727", "v3.0", "v1.0.3705", "v1.1.4322")

    For Each dotNetVersion In NetVersions
        If FSO.FileExists(sFrameWork & dotNetVersion & "\RegAsm.exe") Then
            RegAsm = DQ(sFrameWork & dotNetVersion & "\RegAsm.exe")
            Exit For
        End If
    Next
    If regAsm = "" Then WScript.StdOut.WriteLine "Microsoft .NET framework is not present on this machine. Please Install it from Microsoft Website."
    If DEBUG_MODE then WScript.StdOut.WriteLine RegAsm

    Set FSO = Nothing
End Function
'*************  Fucntion to UnRegister the Utility If Present  *************************************************
Function UninstallDotNetAssembly(foldername,dllFileName,tlbName)
    Set WShell = CreateObject("WScript.Shell")
    fileName = DQ(folderName & dllFileName)
    TLBFileName = DQ(folderName & tlbName)
    cmd = Replace(UNREGISTER_COMMAND, "%REGASM%", regAsm)
    cmd = Replace(cmd, "%FILENAME%", fileName)
    cmd = Replace(cmd, "%TLBFILENAME%", TLBfileName)
    If DEBUG_MODE Then WScript.StdOut.WriteLine cmd
    WSHell.Run cmd, , True
End Function
'*************  Fucntion to Register the Utility *************************************************
Function InstallDotNetAssembly(foldername,dllFileName,tlbName)
    If DEBUG_MODE Then WScript.StdOut.WriteLine folderName

    Set WShell = CreateObject("WScript.Shell")
    fileName = DQ(folderName & dllFileName)
    TLBFileName = DQ(folderName & tlbName)

    cmd = Replace(REGISTER_COMMAND, "%REGASM%", regAsm)
    cmd = Replace(cmd, "%FILENAME%", fileName)
    cmd = Replace(cmd, "%TLBFILENAME%", TLBfileName)
    If DEBUG_MODE Then WScript.StdOut.WriteLine cmd
    WSHell.Run cmd, , True
End Function

Function DQ(byVal strText)
    DQ = Chr(34) & strText & Chr(34)
End Function
'************************************************** Script Starts from here *******************************************
on error resume next

if DEBUG_MODE Then WScript.StdOut.WriteLine "ENTER"
Dim foldername

If IsObject(Session) Then
    if DEBUG_MODE Then WScript.StdOut.WriteLine "in if"
    If DEBUG_MODE Then WScript.StdOut.WriteLine Session.Property("APPDIR")
	
    folderName = Session.Property("APPDIR")
Else
    if DEBUG_MODE Then WScript.StdOut.WriteLine "in Else"
    If DEBUG_MODE Then WScript.StdOut.WriteLine WScript.ScriptFullName
    If DEBUG_MODE Then WScript.StdOut.WriteLine WScript.ScriptName
    folderName = Left(WScript.ScriptFullName, Len(WScript.ScriptFullName) - Len(WScript.ScriptName))
End If

Call FindRegAsm()
Call UninstallDotNetAssembly(foldername,"WellFloUI.dll","WellFloUI.tlb")
Call InstallDotNetAssembly (foldername,"WellFloUI.dll","WellFloUI.tlb")
if DEBUG_MODE Then WScript.StdOut.WriteLine "EXIT"

