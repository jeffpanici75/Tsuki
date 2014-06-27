Set objFS = CreateObject("Scripting.FileSystemObject")
Set objArgs = WScript.Arguments

If objArgs.length = 0 Then 
	WScript.Quit 0
End If

strFile1 = objArgs(0)
strFile2 = objArgs(1)

WScript.Echo "Check " & strFile1
If Not objFS.FileExists(strFile1) Then 
	WScript.Echo "...Source file missing, do nothing."
	WScript.Quit 0
Else
	WScript.Echo "...found!"
End If
WScript.Echo "Check " & strFile2
If Not objFS.FileExists(strFile2) Then 
	WScript.Echo "...Target file missing, buld."
	WScript.Quit 1
Else
	WScript.Echo "...found!"
End If

Set objFile1 = objFS.GetFile(strFile1)
Set objFile2 = objFS.GetFile(strFile2)

If objFile1.DateLastModified < objFile2.DateLastModified Then
    WScript.Echo "Source file is older than target file; no build."
    WScript.Quit 0
Else
    WScript.Echo "Source file is newer than target file; run build."
    WScript.Quit 1
End If