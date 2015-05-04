
:: this script copys a dll which was refusing to automatically go into the build folde

set SolutionPath=%1
set ProjectPath=%2
set ConfigurationName=%3

set LibFolder=Lib

set CopyFrom=%SolutionPath%%LibFolder%
set CopyTo=%ProjectPath%bin\%ConfigurationName%

xcopy %CopyFrom% %CopyTo% /Y




