'*********************  Test & Results Path Configuiration ***********************************
	basepath="C:\Project\WellFlo\Tests_with_SOR\"
	resultspath="C:\Project\WellFlo\Test_Results\"
	sequencefile="C:\Project\WellFlo\seq1.xls"
'*********************  Test & Results Path Configuiration ***********************************

Dim testcount
testcount=1
pcount=0
fcount=0
resultxlsfile=Inputbox("Please Enter File Name where Test Results would be created ", "Enter Results File Path","C:\Project\WellFlo\Test_Results\resultsQTP.xls")

set fso=createobject("Scripting.FileSystemObject")
If fso.fileexists(resultxlsfile) Then
'	sfname=split(resultxlsfile,".",-1,1)
'	sfname(0)=sfname(0)&tempfilename
'	sfname1=join(sfname,".")
     str_dot_pos=InStrRev(resultxlsfile,".")
     str_bksl_pos=InStrRev(resultxlsfile,"\")
     sn_string=Mid(resultxlsfile,str_bksl_pos+1,str_dot_pos-str_bksl_pos-1)
      sn_string= sn_string&tempfilename
      sn_string_1=Mid(resultxlsfile,1,str_bksl_pos)
      sn_string_3=Mid(resultxlsfile,str_dot_pos,Len(resultxlsfile)-str_bksl_pos)
      mod_file_name=sn_string_1&sn_string&sn_string_3
	fso.copyfile resultxlsfile , mod_file_name
	fso.deletefile(resultxlsfile) 
End If




'tb1=timer
bstart=now


Set oxl=CreateObject("Excel.application")
Set owb=oxl.workbooks.open(sequencefile)
Set osht=owb.worksheets(1)

'=====================================================================
For i= 2 to osht.UsedRange.rows.count
      If osht.cells(i,3).value="Y" Then
      ExecuteTest basepath&osht.cells(i,1).value ,resultspath&osht.cells(i,2).value
      End If
Next
'=====================================================================

owb.close
oxl.quit
Set osht=nothing
Set owb=nothing
Set oxl=nothing

'*************  Write Overall Batch Summary in Second Excel Sheet *************************
bend=now
'tb2=timer
'tbt=tb2-tb1
tbt=datediff("s",bstart,bend)
Set objxls=createobject("Excel.Application")
set objworkbook=objxls.workbooks.open  (resultxlsfile)
Set objSheet2 = objworkbook.Worksheets(2)

objSheet2.Cells(1, 1).Value = "No of Test Scripts Executed : "
objSheet2.Cells(2, 1).Value = "No of Test Scripts Passed   : "
objSheet2.Cells(3, 1).Value = "No of Test Scripts Failed   : "
objSheet2.Cells(4, 1).Value = "Test Execution Time ( in secs) "
objSheet2.Cells(5, 1).Value = "Test Started At"
objSheet2.Cells(6, 1).Value = "Test Ended At"

objSheet2.Cells(1, 2).Value = testcount -1
objSheet2.Cells(2, 2).Value = pcount
objSheet2.Cells(3, 2).Value = fcount
objSheet2.Cells(4, 2).Value = tbt
objSheet2.Cells(5, 2).Value = "'"&bstart
objSheet2.Cells(6, 2).Value = "'"&bend
objworkbook.save
objworkbook.close
objxls.quit
Set objxls=nothing
'*************  
'*****************************  Call All Test Scripts of WellFlo in sequence ************************'

msgbox "Batch Test Execution is Complete... Please Check Results File  "& resultxlsfile&" To know Status !! "

'***************************************  End of Main script ********** 



'*************************************** Execute Test Function *********
Public Sub ExecuteTest(TestPath,respath)   
	Dim qtApp 
	Dim qtTest 
	Dim qtResultsOpt
	On Error Resume Next
	' == Create the Application object ==
	Set qtApp = CreateObject("QuickTest.Application") 
	If Err.number <> 0 Then
		Msgbox "QTP Application is Not installed on your machine Please Install it and then run the script "
		wscript.quit
	End if
	' == Start QuickTest ===
	qtApp.Launch 
	'== Make the QuickTest application visible ==
	qtApp.Visible = True 
	' Set QuickTest run options
	qtApp.Options.Run.ImageCaptureForTestResults = "OnError"
	qtApp.Options.Run.RunMode = "Fast"
	qtApp.Options.Run.ViewResults = False
	'== Open the test in read-only mode ==
	qtApp.Open TestPath, False 
	'== Set run settings for the test ==
	Set qtTest = qtApp.Test
	'== Create the Run Results Options object ==
	Set qtResultsOpt = CreateObject("QuickTest.RunResultsOptions")
	' Set the results location 
	qtResultsOpt.ResultsLocation = respath 
	sttime=now
	't1=timer
	qtTest.Settings.Run.IterationMode = "rngIterations"
	qtTest.Settings.Run.StartIteration = 1
	qtTest.Settings.Run.EndIteration = 1
	qtTest.Run qtResultsOpt ' Run the test
	't2=timer
	endtime=now
	'diff=t2-t1
     diff=datediff("s",sttime,endtime)
	'''Call a function to write test results in an Excel file
	result=qtTest.LastRunResults.Status
	resultexcel testpath,result,diff,sttime,endtime
    ' Close the test
	qtTest.Close 
	wscript.sleep 3000
	Set qtResultsOpt = Nothing ' Release the Run Results Options object
	Set qtTest = Nothing ' Release the Test object
	Set qtApp = Nothing ' Release the Application object  
End Sub
'*************************************** Execute Test Function *************


'******************************* Write QTP TEST Results in an Excel File ******
Public Function resultexcel(stxt1,stxt2,stxt3,stxt4,stxt5)
	Set objxls=createobject("excel.application")

	If not fso.fileexists(resultxlsfile) then
		objxls.Workbooks.Add
		Set objSheet = objxls.ActiveWorkbook.Worksheets(1)
		Set objSheet2 = objxls.ActiveWorkbook.Worksheets(2)
	Else
		set objworkbook=objxls.workbooks.open  (resultxlsfile)
		Set objSheet = objworkbook.Worksheets(1)
		Set objSheet2 = objworkbook.Worksheets(2)
	End If
	
	' Bind to worksheet.
	objSheet.Name = "Test Details"
	objSheet2.Name="Test Summary"
	' Populate spreadsheet cells with user attributes.
	objSheet.Cells(1, 1).Value = "Test  Case ID"
	objSheet.Cells(1, 2).Value = "Test Case Title"
	objSheet.Cells(1, 3).Value = "Status"
	objSheet.Cells(1, 4).Value = "Test Execution Time ( in secs) "
	objSheet.Cells(1, 5).Value = "Test Started At"
	objSheet.Cells(1, 6).Value = "Test Ended At"
	objSheet.Range("A1:F1").Font.Bold = True
	objSheet.Range("A1:F1").Interior.colorindex=6
	stxtf=regexptest("T\d.*",stxt1)
	testcount= testcount + 1
	objSheet.Cells(testcount,1).Value = testcount -1 
	objSheet.Cells(testcount,2).Value = stxtf
	objSheet.Cells(testcount,3).Value = stxt2
	objSheet.Cells(testcount,4).Value = stxt3
	objSheet.Cells(testcount,5).Value = "'"&stxt4
	objSheet.Cells(testcount,6).Value = "'"&stxt5
	objxls.Columns(1).ColumnWidth = 12
	objxls.Columns(2).ColumnWidth = 30
	objxls.Columns(3).ColumnWidth = 20
	objxls.Columns(4).ColumnWidth = 20
	objxls.Columns(5).ColumnWidth = 40
	objxls.Columns(6).ColumnWidth = 40
	
	If stxt2="Passed" Then
		objSheet.Cells(testcount,3).interior.colorindex=4
		pcount=pcount+1
	Else
		objSheet.Cells(testcount,3).interior.colorindex=3
		fcount=fcount +1
	End If
	
	If not fso.fileexists(resultxlsfile) then
		objxls.ActiveWorkbook.SaveAs resultxlsfile
		objxls.ActiveWorkbook.Close
		objxls.Application.Quit
	Else
		objworkbook.save
		objxls.displayAlerts = False 
		objworkbook.close
		objxls.quit
	End If
	
	Set objxls=nothing ' Release evey excel object also
	
End Function

'************ Write QTP TEST Results in an Excel File ***********


'**********************  RegExp TEST *************************************************************
Function RegExpTest(patrn, strng)
   Dim regEx, Match, Matches   ' Create variable.
   Set regEx = New RegExp   ' Create regular expression.
   regEx.Pattern = patrn   ' Set pattern.
   regEx.IgnoreCase = True   ' Set case insensitivity.
   regEx.Global = True   ' Set global applicability.
   Set Matches = regEx.Execute(strng)   ' Execute search.
   For Each Match in Matches   ' Iterate Matches collection.
      'RetStr = RetStr & "Match found at position "
      'RetStr = RetStr & Match.FirstIndex & ". Match Value is '"
      RetStr = RetStr&Match.Value
   Next
   RegExpTest = RetStr
End Function
'**********************  RegExp TEST *************************************************************


''*********************  Temp File Name ***********************************************************
Function tempfilename
	Dim curDate
	'Get the current date
	curDate = Now 
	hh=Hour(curDate)
	mm=minute(curDate)
	ss=second(curDate)
	dd=day(curDate)
	mt=month(curDate)
	yyyy=year(curDate)
  
	If len(hh)=1   Then
         hh=0&hh
	End If
	If len(mm)=1   Then
         mm=0&mm
	End If
	If len(ss)=1   Then
         ss=0& ss
	End If
	If len(dd)=1   Then
         dd =0&dd 
	End If
	If len(mt)=1   Then
         mt =0&mt
	End If
	GetRandomNumberByDate = hh & _
			   mm & _
				ss & _
				dd & _
				mt & _
				yyyy
    tempfilename=GetRandomNumberByDate
End Function
'**************************** Temp file Name **************************************************************
