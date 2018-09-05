#region

using System.Collections.Generic;
using JosephM.Application.ViewModel.RecordEntry.Metadata;

#endregion

namespace JosephM.Application.ViewModel.Fakes
{
    public static class FakeConstants
    {
        public const string RecordType = "TestType";
        public const string Id = "Id";
        public const string PrimaryField = "PrimaryField";
        public const string IntegerField = "IntegerField";
        public const string StringField = "StringField";
        public const string EnumerableField = "EnumerableField";
        public const string BooleanField = "BooleanField";
        public const string HtmlField = "HtmlField";
        public const string ComboField = "ComboField";
        public const string DateOfBirthField = "DateOfBirthField";
        public const string AgeField = "AgeField";
        public const string LookupField = "LookupField";
        public const string DecimalField = "DecimalField";
        public const string DoubleField = "DoubleField";
        public const string MoneyField = "MoneyField";
        public const string PasswordField = "PasswordField";
        public const string FakeLookupId = "Id";
        public const string FakeName = "Fake Record!";
        public const string IntersectField1 = Id + "1";
        public const string IntersectField2 = Id + "2";
        public const string IntersectRecordName = RecordType + "to" + RecordType;

        public const string RecordType2 = "TestType2";
        public const string Id2 = "Id2";
        public const string PrimaryField2 = "PrimaryField2";
        public const string FakeId2 = "qaz";

        public const string FakePostType = "post";
        public const string FakePostTypeId = "postid";
        public const string FakePostTypePrimaryField = "text";

        public const string FakeEmailType = "email";
        public const string FakeEmailTypeId = "emailid";
        public const string FakeEmailTypePrimaryField = "subject";
        public const string FakeEmailTypeContentField = "description";
        public const string FakeEmailTypeActivityParty = "activityparty";

        public const string FakeContactType = "contact";
        public const string FakeContactTypeId = "contactid";
        public const string FakeContactPrimaryField = "name";

        public const string FakeActivityPartyType = "activityparty";
        public const string FakeActivityPartyTypeId = "activitypartyid";
        public const string FakeActivityPartyTypePrimaryField = "name";
        public const string FakeActivityPartyTypePartyField = "partyid";
        public const string FakeActivityPartyTypeActivityField = "activityid";

        public static IEnumerable<GridFieldMetadata> FakeGridFields
        {
            get
            {
                return new[]
                {
                    new GridFieldMetadata(Id),
                    new GridFieldMetadata(StringField) {WidthPart = 3, IsEditable = true},
                    new GridFieldMetadata(IntegerField),
                    new GridFieldMetadata(BooleanField),
                    new GridFieldMetadata(EnumerableField)
                };
            }
        }

        public const string AllFieldTypesType = "AllFieldTypesType";
        public const string AllFieldTypesTypeLabel = "All Field Types";
        public const string AllId = "Id";
        public const string AllMemo = "Memo";
        public const string AllString = "String";
        public const string AllPicklist = "Picklist";
        public const string AllInteger = "Integer";
        public const string AllBoolean = "Boolean";
        public const string AllDate = "Date";
        public const string AllLookup = "Lookup";
        public const string AllDecimal = "Decimal";
        public const string AllMoney = "Money";
        public const string AllPassword = "Password";
        public const string AllFolder = "Folder";
        public const string AllDouble = "Double";
        public const string AllRecordType = "RecordType";

        public const string MainRecordName = "Main Record";


        public const string FakeHtml =
            @"<!DOCTYPE html PUBLIC '-//W3C//DTD HTML 4.01//EN' 'http://www.w3.org/TR/html4/strict.dtd'>
<!-- saved from url=(0115)https://mail.google.com/mail/u/0/?ui=2&ik=33249021d4&view=pt&search=inbox&th=144e182a786df1c8&siml=144e182a786df1c8 -->
<html><head><meta http-equiv='Content-Type' content='text/html; charset=UTF-8'>

<meta http-equiv='X-UA-Compatible' content='IE=edge'>
<title>Gmail - Football fans, FIFA free-to-play is here!</title>
<style type='text/css'>
body, td{font-family:arial,sans-serif;font-size:13px} a:link, a:active {color:#1155CC; text-decoration:none} a:hover {text-decoration:underline; cursor: pointer} a:visited{color:##6611CC} img{border:0px} pre { white-space: pre; white-space: -moz-pre-wrap; white-space: -o-pre-wrap; white-space: pre-wrap; word-wrap: break-word; max-width: 800px; overflow: auto;}
</style>
<script type='text/javascript'>// <![CDATA[
function Print(){document.body.offsetHeight;window.print()};
// ]]>
</script>
</head>
<body onload='Print()'>
<div class='bodycontainer'>
<table width='100%' cellpadding='0' cellspacing='0' border='0'>
<tbody><tr height='14px'>
<td width='143'>
<img src='./Gmail - Football fans, FIFA free-to-play is here!_files/logo1.gif' width='143' height='59' alt='Gmail'>
</td>
<td align='right'>
<font size='-1' color='#777'><b>
Joseph McGregor
&lt;josephmcmac@gmail.com&gt;
</b></font></td>
</tr>
</tbody></table>
<hr>
<div class='maincontent'>
<table width='100%' cellpadding='0' cellspacing='0' border='0'>
<tbody><tr>
<td>
<font size='+1'>
<b>Football fans, FIFA free-to-play is here!</b></font><br>
<font size='-1' color='#777'>1 message</font>
</td>
</tr>
</tbody></table>
<hr>
<table width='100%' cellpadding='0' cellspacing='0' border='0' class='message'>
<tbody><tr>
<td>
<font size='-1'><b>
FIFA World
</b>
&lt;fifaworld@em.ea.com&gt;
</font>
</td>
<td align='right'>
<font size='-1'>
Fri, Mar 21, 2014 at 9:00 AM
</font>
</td></tr><tr>
<td colspan='2'>
<font size='-1' class='recipient'>
<div class='replyto'>
Reply-To:
FIFA World &lt;support-b61u6xvbgwmbfsauyc42fqk08hmp7k@em.ea.com&gt;
</div>
<div>
To:
joe@mc-mac.com
</div>
</font>
</td></tr><tr>
<td colspan='2'>
<table width='100%' cellpadding='12' cellspacing='0' border='0'>
<tbody><tr>
<td>
<div style='overflow: hidden;'>
<font size='-1'><u></u>
<div bgcolor='#FFFFFF' marginwidth='0' marginheight='0' align='center'>
<table align='center' border='0' width='640' cellpadding='0' cellspacing='0'>
<tbody><tr><td width='640' align='center' valign='top'>
<table align='center' width='640' border='0' cellpadding='0' cellspacing='0'>
<tbody>
<tr>
<td width='640' align='left'>
<table align='center' width='640' border='0' cellpadding='0' cellspacing='0' bgcolor='#ffffff'>
<tbody>
<tr>
<td height='10' colspan='2'></td>
</tr>
<tr>
<td align='left' colspan='2'>
<font style='font-size:12px;font-family:Arial,sans-serif;color:#4b565a'>
<strong>&nbsp;&nbsp;Take your passion for football to new heights.&nbsp;<a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a2?NUCLEUS_ID=1000120385498' style='color:#1979ff;text-decoration:underline' target='_blank'>Play EA SPORTS FIFA World now!</a></strong>
</font>
</td>
</tr>
<tr>
<td width='640' height='18' align='left'>
<font style='font-size:9px;font-family:Arial,sans-serif;color:#4b565a'>
&nbsp;&nbsp;If you are having trouble viewing this email, please&nbsp;<a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a1?t_params=EMAIL%3Djoe%2540mc-mac.com%26NUCLEUS_ID%3D1000120385498&NUCLEUS_ID=1000120385498' style='color:#4b565a;text-decoration:underline' target='_blank'>click here</a>.
</font>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
</tbody>
</table>
<table align='center' width='640' border='0' cellpadding='0' cellspacing='0' bgcolor='#ffffff'>
<tbody>
<tr>
<td><a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a2?NUCLEUS_ID=1000120385498' target='_blank'><img border='0' style='display:block' width='640' height='456' alt='FIFA WORLD' src='./Gmail - Football fans, FIFA free-to-play is here!_files/aEqxJ8WDJ8NAqxZmbNbtxuc4ZZZwB7Jl3oP3ll5a4M5SlP07i8pjZAKY0uLcJ1ZcTYcxyH-c4_Q3l5FpeFW9WL8RBysG5AbYr3svOp57C1KhmH924ne1Rs9juLl_9Q=s0-d-e1-ft'></a>
</td>
</tr>
<tr>
<td>
<table align='center' width='640' border='0' cellpadding='0' cellspacing='0' bgcolor='#f0f0f0'>
<tbody>
<tr>
<td><img border='0' style='display:block' width='29' height='308' alt='' src='./Gmail - Football fans, FIFA free-to-play is here!_files/biMOPt8TbESbbzqpK5zL-KgulWjlZzsOW9cPle1wRs2jbH3Q1xRutBz4lkLV3e0pqIPsM5Empv97ko_HMh8D7Oa2aK7Wr8DaFUd1oDeTvpaSAJX75710VtOsLwI=s0-d-e1-ft'>
</td>
<td width='317' valign='top' align='left' style='display:block'>
<font style='font-size:18px;font-family:Arial,sans-serif;color:#454545'><strong style='font-size:24px;color:#000'>Take to the pitch</strong>
<table><tbody><tr><td height='5'></td></tr></tbody></table>
Play as your favourite club in Online Seasons or create your own unique squad to play your style of football in Ultimate Team. <table><tbody><tr><td height='5'></td></tr></tbody></table>
Challenge your friends or challenge the world – it’s the authentic FIFA experience, now FREE to play on PC.
<table><tbody><tr><td height='5'></td></tr></tbody></table>
<a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a2?NUCLEUS_ID=1000120385498' style='color:#2175ff;text-decoration:underline;font-size:18px' target='_blank'><strong>PLAY FOR FREE NOW!</strong></a>
</font>
</td>
<td><img border='0' style='display:block' width='294' height='308' alt='' src='./Gmail - Football fans, FIFA free-to-play is here!_files/G5so-kIYaS1OjrdBKsRin-ZKE88o3k43qIFvV0qLJjOBVnAOUdwF8BmtHAh7KG2iL7dPq7DTgNgVdPB__RBd8LOpPUjnYxfCGWTcUvUn-cXv4QbpsHjNsVUnOhC5=s0-d-e1-ft'>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
<tr>
<td>
<table align='center' width='640' border='0' cellpadding='0' cellspacing='0'>
<tbody>
<tr>
<td><img border='0' style='display:block' width='75' height='338' alt='' src='./Gmail - Football fans, FIFA free-to-play is here!_files/biG07jRRdEeq8c41BUeTxnlbnYMGErd0GFFoHIuSkL0iLPEqxo-0PfgvQ4e_kR8XhiryMJJa-NxrXG4UARFzpRQV4v60rdsFs9KI-6Iq7qO5vI9I5QYtNAsiEGI=s0-d-e1-ft'>
</td>
<td><a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a3?NUCLEUS_ID=1000120385498' target='_blank'><img border='0' style='display:block' width='498' height='338' alt='EA SPORTS FIFA WORLD TRAILER' src='./Gmail - Football fans, FIFA free-to-play is here!_files/1F-AhOdxL09p5ZYfhMBxz4FvI0oynHv4vT5KvtOoOEbBYxjvIE9he6-cM0YA29yTLPene6Nw-fG91SPz38H_i5kInlZ0um-qHD1XEeBKaCV8SCGsJPH2FmgoBAPzlVI=s0-d-e1-ft'></a>
</td>
<td><img border='0' style='display:block' width='67' height='338' alt='' src='./Gmail - Football fans, FIFA free-to-play is here!_files/v7gKml88EAuDr0FNxo1vsTBZ0Cyersm7z8CWzy_oIkQrIKCbUM09RDEmy3k2MAADzf4RH8hsKpHvHZ6vYb81nRNWNB7gR7MfDuhIU-7A1VRV_qa3ox-uEecNNRS5=s0-d-e1-ft'>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
<tr>
<td>
<table align='center' width='640' border='0' cellpadding='0' cellspacing='0' bgcolor='#ffffff'>
<tbody>
<tr>
<td><img border='0' style='display:block' width='88' height='163' alt='' src='./Gmail - Football fans, FIFA free-to-play is here!_files/Kw7sCSHVngeri5XyAkS0RdbdmVmtCPjm1w6jlE7oGeh7L4rzk4jMegXTRQq55WF_MJye8KtXYthFTRzQzqCe3oVCvasp2h5urCxYEVhzW33OwZT87U0cu-jpv2o=s0-d-e1-ft'>
</td>
<td width='470' align='center' style='display:block' valign='top'>
<font style='font-size:18px;font-family:Arial,sans-serif;color:#454545'>EA SPORTS™ FIFA World features award-winning
EA SPORTS FIFA HD console gameplay and over 30 officially licensed leagues, 600 clubs and 16,000 players. Connect with friends or compete against the world as you rise through the rankings.
<table><tbody><tr><td height='5'></td></tr></tbody></table>
<a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a3?NUCLEUS_ID=1000120385498' style='color:#2175ff;text-decoration:underline;font-size:18px' target='_blank'><strong>WATCH THE VIDEO</strong></a>
</font>
</td>
<td><img border='0' style='display:block' width='82' height='163' alt='' src='./Gmail - Football fans, FIFA free-to-play is here!_files/8KJkaVEWNFuc3DAYQcnwrzUR8taKzI6NQ3FlfZTBRvYcbJxEgX0Ld3wbXc_zaf9EScZe7mgh2TnzRHBWUu_V08W4qYl4EyReU4n4p4AzBRnfVZKSXkuxZ53lMgVJ=s0-d-e1-ft'>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
<tr>
<td><img border='0' style='display:block' width='640' height='54' alt='' src='./Gmail - Football fans, FIFA free-to-play is here!_files/sKIhkKH0hrgg5g7Ngi9c_nqPKQXGNjtfGlebYXsHz6MZEhLk6oqOnsaDDhKR1Su4icR3iA3SIvrOR-kLFwwgQV3q-EmtlSkom_KY3_5fuinur0t0ND6VGVQ6CQ=s0-d-e1-ft'>
</td>
</tr>
<tr>
<td>
<table align='center' width='640' border='0' cellpadding='0' cellspacing='0'>
<tbody>
<tr>
<td><a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a13?NUCLEUS_ID=1000120385498' target='_blank'><img border='0' style='display:block' width='229' height='47' alt='FACEBOOK' src='./Gmail - Football fans, FIFA free-to-play is here!_files/M2vMDOfoOIcSRJPhEsYW7UGwqdddA2iNc6-rTRGqREpv_wpUp1Pxb46AlBAgWtxDaJSBAf1TFEWSvwiQMzZ71tWNsVEJn6I3NWZ4a68NWTl_SuN1mblA3rWkygb7pUk=s0-d-e1-ft'></a>
</td>
<td><a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a12?NUCLEUS_ID=1000120385498' target='_blank'><img border='0' style='display:block' width='188' height='47' alt='TWITTER' src='./Gmail - Football fans, FIFA free-to-play is here!_files/plINJ8lJHOarJsFrxTFKKQ8CicYc-SJkdrIYLzzlhApqzcgA7sMQ5ThtzqU1Hs9q7ELrp-NCNhvyll3WWC_QvE4Cwx3cqYtbVpk1PiN2-EMcsgTKUoMGhSxv6yxEUQ=s0-d-e1-ft'></a>
</td>
<td><a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a11?NUCLEUS_ID=1000120385498' target='_blank'><img border='0' style='display:block' width='223' height='47' alt='YOUTUBE' src='./Gmail - Football fans, FIFA free-to-play is here!_files/DjwkwhFCGQZBSer1Xh_mzd0-xmdhJbwArOTAuEhb_CXxWBmxAdUFs1HpjfDtOQSVIpo3MOm_oMCYgZfMl3LM88SilN8LrGUr7k3AuwSNoxvkdwM7k-QSu4Kb0PKsuA=s0-d-e1-ft'></a>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
<tr>
<td>
<img border='0' style='display:block' width='640' height='23' alt='' src='./Gmail - Football fans, FIFA free-to-play is here!_files/qmb04EQv_uVaagGe6dUNZNn-ErYc_7aWoNws6CmQTy8mXqf6E531iI8blOTYbr8iIWtDliVTlIA2_CPvir09DB-LeBxNal0MEdOgbfizDUVonf6cwbBLGkcx9A=s0-d-e1-ft'>
</td>
</tr>
</tbody>
</table>
<table width='640' border='0' cellpadding='0' cellspacing='0' bgcolor='ffffff' align='center'>
<tbody><tr><td height='10'></td></tr>
<tr>
<td width='640' align='center'>
<font style='font-size:11px;font-family:Arial,sans-serif;color:#37342e'>
Make sure you receive your email from FIFA WORLD! Add <a href='mailto:FIFAWORLD@em.ea.com' style='color:#37342e;text-decoration:underline' target='_blank'>FIFAWORLD@em.ea.com</a> to your address book.
</font>
</td>
</tr>
<tr><td height='20'></td></tr>
<tr>
<td width='640' align='center'>
<font style='font-size:11px;font-family:Arial,sans-serif;color:#37342e'>
If you no longer want us to contact you, <a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a4?email=joe@mc-mac.com&NUCLEUS_ID=1000120385498' style='color:#37342e;text-decoration:underline' target='_blank'>click here</a> to be removed from our mailing list or to change your preferences. You can also write to: Privacy Policy Administrator, Electronic Arts Inc., 209 Redwood Shores Parkway, Redwood City, CA 94065.
</font>
</td>
</tr>
<tr><td height='20'></td></tr>
<tr>
<td width='640' align='center'>
<table align='center' cellspacing='0' cellpadding='0' border='0'>
<tbody>
<tr>
<td width='35'>
<a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a10?NUCLEUS_ID=1000120385498' target='_blank'>
<img border='0' width='35' height='27' alt='play4free.com' src='./Gmail - Football fans, FIFA free-to-play is here!_files/dWqzjmQsw_iKeV43AAzViOvFxF7L01PpvU2fzMbTMWuywyzcTESuDNusfBQxPNS6wQk1RXwKI-f6MytYa5ttZpIIttQXaYjLk8uAI1U0YoM=s0-d-e1-ft'>
</a>
</td>
<td width='39'>
<a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a5?NUCLEUS_ID=1000120385498' target='_blank'>
<img border='0' width='39' height='27' alt='ea.com' src='./Gmail - Football fans, FIFA free-to-play is here!_files/28z2P7bI1opJsuBe_3gxvXS3i94gIaRSu0L2W3NpG-57xrk6xWOjt4V66dKODuXO4IANFj3yJ5LNlQYjV_EXnh_dNuglv7CJbg=s0-d-e1-ft'>
</a>
</td>
<td valign='middle'>
<font style='font-size:11px;font-family:Arial,sans-serif;color:#37342e'>
©2014 Electronic Arts Inc. All Rights Reserved.
<a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a6?NUCLEUS_ID=1000120385498' target='_blank'><span style='color:#37342e;text-decoration:underline'>Legal Notice</span></a> |
<a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a7?NUCLEUS_ID=1000120385498' target='_blank'><span style='color:#37342e;text-decoration:underline'>Privacy Policy</span></a> |
<a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a8?NUCLEUS_ID=1000120385498' target='_blank'><span style='color:#37342e;text-decoration:underline'>Terms of Service</span></a>
</font>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
<tr><td height='20'></td></tr>
<tr>
<td width='640' align='center'>
<a href='http://em.ea.com/a/hBTK2TlB7uauvB849znN7ioOUtf/a9?NUCLEUS_ID=1000120385498' target='_blank'><img border='0' alt='' src='./Gmail - Football fans, FIFA free-to-play is here!_files/UWK926T4rpmCU_uW3X4EvG-LiuCTX2uIT_U34axbxI8JrEVppSj4l2IEfgKT1A90znCfsk1H6q-f5wSYvBLu_A=s0-d-e1-ft' style='display:block'></a>
</td>
</tr>
<tr><td height='10'></td></tr>
<tr>
<td width='640' align='center'>
<font style='font-size:11px;font-family:Arial,sans-serif;color:#37342e'>
PERSISTENT INTERNET CONNECTION, ORIGIN ACCOUNT, ACCEPTANCE OF PRODUCT AND ORIGIN END USER LICENSE AGREEMENTS (EULAS) AND INSTALLATION OF THE ORIGIN CLIENT SOFTWARE (<a href='http://www.origin.com/ABOUT' target='_blank'>WWW.ORIGIN.COM/ABOUT</a>) REQUIRED TO PLAY. YOU MUST BE 13+. EA ONLINE PRIVACY POLICY AND TERMS OF SERVICE CAN BE FOUND AT <a href='http://www.ea.com/' target='_blank'>WWW.EA.COM</a>. EULAS AND ADDITIONAL DISCLOSURES CAN BE FOUND AT <a href='http://www.ea.com/1/PRODUCT-EULAS' target='_blank'>WWW.EA.COM/1/PRODUCT-EULAS</a>. EA MAY RETIRE ONLINE FEATURES AND SERVICES AFTER 30 DAYS NOTICE POSTED ON <a href='http://www.ea.com/1/SERVICE-UPDATES' target='_blank'>WWW.EA.COM/1/SERVICE-UPDATES</a>.
</font>
</td>
</tr>
<tr><td height='50'></td></tr>
</tbody></table>
</td></tr></tbody></table>
<img src='./Gmail - Football fans, FIFA free-to-play is here!_files/pFAXo-K-D5gPuYouogDZ6H4XaveHl9eoRNiwMqG6K5G6kSdCdLEqo3h7Fc1m0OyMlamO-0-OAWSTa_Ckt6hOM907bd5LlHZhVb7xOmbotQ=s0-d-e1-ft'>
</div>
</font>
</div>
</td></tr></tbody></table>
</td></tr></tbody></table>
</div>
</div>


</body></html>
";
    };
}