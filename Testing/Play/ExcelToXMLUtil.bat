set utillocation="C:\Prasanna\ForeSite\UI Automation\Helper Projects\ExceltoXML\ExceltoXML\bin\Debug"
set filelocation="C:\Workspace\Perforce\ForeSite_PH\UQA\AssetsDev\UIAutomation\TestData"
cd /D %utillocation%
REM ExceltoXML.exe %filelocation%\DHPump1.xls "DHPump" "record" %filelocation%\DHPump1.xml
REM ExceltoXML.exe %filelocation%\DHPump2.xls "DHPump" "record" %filelocation%\DHPump2.xml
REM ExceltoXML.exe %filelocation%\DHPump3.xls. "DHPump" "record" %filelocation%\DHPump3.xml
ExceltoXML.exe %filelocation%\Load1.xls "Load" "record" %filelocation%\Load1.xml
ExceltoXML.exe %filelocation%\Load3.xls "Load" "record" %filelocation%\Load3.xml
