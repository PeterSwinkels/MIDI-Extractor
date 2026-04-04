'This module's imports and settings.
Option Compare Binary
Option Explicit On
Option Infer Off
Option Strict On

Imports System
Imports System.Convert
Imports System.Environment
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Encoding

'This module contains this program's core procedures.
Public Module CoreModule
   Private Const MIDI_HEADER As String = "MThd"   'Defines the MIDI header signature.
   Private Const MIDI_TRACK As String = "MTrk"   'Defines the MIDI track signature.

   'This procedure returns the specified bytes as text.
   Private Function BytesToText(Bytes() As Byte, Offset As Integer, Length As Integer) As String
      Try
         Dim Text As New StringBuilder

         For Position As Integer = Offset To Offset + Length - &H1%
            If Position >= Bytes.Length Then Exit For
            Text.Append(ToChar(Bytes(Position)))
         Next Position

         Return Text.ToString()
      Catch ExceptionO As Exception
         DisplayException(ExceptionO)
      End Try

      Return ""
   End Function

   'This procedure displays any exceptions that occur.
   Private Sub DisplayException(ExceptionO As Exception)
      Try
         Console.WriteLine($"ERROR: {ExceptionO.Message}")
      Catch
         [Exit](0)
      End Try
   End Sub

   'This procedure returns the end of the specified MIDI data.
   Private Function GetMIDIEnd(Data() As Byte, Offset As Integer) As Integer
      Try
         Dim ChunkID As String = Nothing
         Dim ChunkLength As New Integer
         Dim ChunkLengthBytes() As Byte = {}
         Dim HeaderLength As New Integer
         Dim HeaderLengthBytes() As Byte = {}
         Dim Position As Integer = Offset + &H4%
         Dim StartOfTracks As New Integer
         Dim TrackCount As New Integer
         Dim TrackCountBytes() As Byte = {}
         Dim TracksFound As Integer = &H0%

         HeaderLengthBytes = {Data(Position), Data(Position + &H1%), Data(Position + &H2%), Data(Position + &H3%)}
         Array.Reverse(HeaderLengthBytes)
         HeaderLength = BitConverter.ToInt32(HeaderLengthBytes, &H0%)
         Position += &H4%

         StartOfTracks = Position + HeaderLength

         TrackCountBytes = {Data(Position + &H2%), Data(Position + &H3%)}
         Array.Reverse(TrackCountBytes)
         TrackCount = BitConverter.ToInt16(TrackCountBytes, &H0%)

         Position = StartOfTracks

         While TracksFound < TrackCount AndAlso Position < Data.Length
            ChunkID = ASCII.GetString(Data, Position, &H4%)
            Position += &H4%

            ChunkLengthBytes = {Data(Position), Data(Position + &H1%), Data(Position + &H2%), Data(Position + &H3%)}
            Array.Reverse(ChunkLengthBytes)
            ChunkLength = BitConverter.ToInt32(ChunkLengthBytes, &H0%)
            Position += &H4%
            Position += ChunkLength

            If ChunkID = MIDI_TRACK Then
               TracksFound += &H1%
            End If
         End While

         Return Position
      Catch ExceptionO As Exception
         DisplayException(ExceptionO)
      End Try

      Return Nothing
   End Function

   'This procedure is executed when this program is started.
   Public Sub Main()
      Try
         Dim Count As Integer = 0
         Dim Data() As Byte = {}
         Dim InputFile As String = Nothing
         Dim OutputFile As String = Nothing
         Dim OutputPath As String = Nothing
         Dim MIDIEnd As New Integer
         Dim MIDIData() As Byte = {}
         Dim Position As New Integer

         Console.WriteLine($"{ProgramInformation()}{NewLine}")

         If GetCommandLineArgs().Count = 3 Then
            InputFile = GetCommandLineArgs(1)
            OutputPath = GetCommandLineArgs(2)
            Data = File.ReadAllBytes(InputFile)

            Position = Data.GetLowerBound(0)
            Do While Position < Data.GetUpperBound(0)
               If BytesToText(Data, Position, MIDI_HEADER.Length) = MIDI_HEADER Then
                  OutputFile = Path.Combine(OutputPath, $"{Count}.mid")
                  MIDIEnd = GetMIDIEnd(Data, Position)
                  If MIDIEnd < Data.Length Then
                     ReDim MIDIData(&H0% To MIDIEnd - Position)
                     Array.Copy(Data, Position, MIDIData, &H0%, MIDIData.Length)
                     File.WriteAllBytes(OutputFile, MIDIData)
                     Console.WriteLine(OutputFile)
                     Count += 1
                     Position += MIDIData.Length
                  End If
               Else
                  Position += &H1%
               End If
            Loop
         Else
            Console.WriteLine($"USAGE: {My.Application.Info.AssemblyName}.exe INPUT_FILE OUTPUT_PATH")
         End If
      Catch ExceptionO As Exception
         DisplayException(ExceptionO)
      End Try
   End Sub

   'This procedure returns information about this program.
   Private Function ProgramInformation() As String
      Try
         Dim Information As String = Nothing

         With My.Application.Info
            Return $"{ .Title} v{ .Version} - by: { .CompanyName}, { .Copyright}"
         End With

         Return Information
      Catch ExceptionO As Exception
         DisplayException(ExceptionO)
      End Try

      Return ""
   End Function
End Module
