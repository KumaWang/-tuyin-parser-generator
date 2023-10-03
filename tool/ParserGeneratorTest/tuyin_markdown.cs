using System;
using SCTE=System.Drawing.Color;
using Tuitor.packages.richtext.format;
using Tuitor.packages.richtext.format.parsers.markdown;
namespace ParserGeneratorTest;
partial class TuyinMarkdownParser
{
  private unsafe char* mInput;
  private int mLength;
  private int mIndex;
  private byte[] _input_498498081=new byte[]{2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1};
  private byte[] _input__1762472396=new byte[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2};
  private byte[] _input__345951309=new byte[]{3,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2};
  private byte[] _input__1722755199=new byte[]{8,0,6,0,0,0,0,0,0,4,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1};
  private byte[] _input__810804434=new byte[]{9,0,7,0,0,0,0,0,0,5,2,0,11,0,0,3,3,3,3,3,3,3,3,3,3,0,0,8,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,6,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,10,0,1};
  private byte[] _input_1589240141=new byte[]{3,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,1};
  private byte[] _input__1525830626=new byte[]{1,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1};
  private readonly Dictionary<ushort,SCTE> __colors=new(){};
  #region ~~
  protected unsafe virtual Match Match_UYIA(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->2
    if(!((c=mInput[mIndex++])==126)){mIndex--;goto LB_0;}
    //2->0
    if(!((c=mInput[mIndex++])==126)){mIndex--;goto LB_0;}
    token=7;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region [|<
  protected unsafe virtual Match Match_U4Zk(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    switch((c=mInput[mIndex++])<=91&&c>=60?_input_498498081[c-60]:0)
    {
      case 1:
        token=9;
        goto LB_0;
      case 2:
        token=13;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region ]
  protected unsafe virtual Match Match_s1tY(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==93)){mIndex--;goto LB_0;}
    token=10;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region (
  protected unsafe virtual Match Match_ZBkV(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==40)){mIndex--;goto LB_0;}
    token=11;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region )
  protected unsafe virtual Match Match_UVNJ(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==41)){mIndex--;goto LB_0;}
    token=12;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region **|__
  protected unsafe virtual Match Match_15gk(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->3
    switch((c=mInput[mIndex++])<=95&&c>=42?_input__1762472396[c-42]:0)
    {
      case 1:
        //3->0
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=15;
        goto LB_0;
      case 2:
        //2->0
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=16;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region **
  protected unsafe virtual Match Match_pkMp(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->2
    if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
    //2->0
    if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
    token=15;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region ***|___
  protected unsafe virtual Match Match_dA5h(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->4
    switch((c=mInput[mIndex++])<=95&&c>=42?_input__1762472396[c-42]:0)
    {
      case 1:
        //4->5
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        //5->0
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=17;
        goto LB_0;
      case 2:
        //2->3
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        //3->0
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region ***
  protected unsafe virtual Match Match_NkI5(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->2
    if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
    //2->3
    if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
    //3->0
    if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
    token=17;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region *|_
  protected unsafe virtual Match Match_9Ftd(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    switch((c=mInput[mIndex++])<=95&&c>=42?_input__1762472396[c-42]:0)
    {
      case 1:
        token=19;
        goto LB_0;
      case 2:
        token=20;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region *
  protected unsafe virtual Match Match_tMId(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
    token=19;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region ++
  protected unsafe virtual Match Match_Fcw1(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->2
    if(!((c=mInput[mIndex++])==43)){mIndex--;goto LB_0;}
    //2->0
    if(!((c=mInput[mIndex++])==43)){mIndex--;goto LB_0;}
    token=21;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region [^
  protected unsafe virtual Match Match_lctR(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->2
    if(!((c=mInput[mIndex++])==91)){mIndex--;goto LB_0;}
    //2->0
    if(!((c=mInput[mIndex++])==94)){mIndex--;goto LB_0;}
    token=22;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region !
  protected unsafe virtual Match Match_4Fpo(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==33)){mIndex--;goto LB_0;}
    token=23;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region |
  protected unsafe virtual Match Match_1UsZ(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==124)){mIndex--;goto LB_0;}
    token=24;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region -
  protected unsafe virtual Match Match_Vgwl(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==45)){mIndex--;goto LB_0;}
    token=25;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region NUMBER
  protected unsafe virtual Match Match_8QYB(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->2
    if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
    token=3;
    LB_2:
    //2->2
    if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
    token=3;
    goto LB_2;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region .
  protected unsafe virtual Match Match_tMkQ(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==46)){mIndex--;goto LB_0;}
    token=26;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region ε
  protected unsafe virtual Match Match_QBpx(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==0)){mIndex--;goto LB_0;}
    token=1;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region ||-|NUMBER
  protected unsafe virtual Match Match_QNNh(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->2
    switch((c=mInput[mIndex++])<=124&&c>=45?_input__345951309[c-45]:0)
    {
      case 1:
        token=3;
        break;
      case 2:
        token=24;
        goto LB_0;
      case 3:
        token=25;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_2:
    //2->2
    if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
    token=3;
    goto LB_2;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region #|[|<|**|__|***|___|*|_|~~|++|[^|!
  protected unsafe virtual Match Match_spQk(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->8
    switch((c=mInput[mIndex++])<=126&&c>=33?_input__1722755199[c-33]:0)
    {
      case 1:
        //8->0
        if(!((c=mInput[mIndex++])==126)){mIndex--;goto LB_0;}
        token=7;
        goto LB_0;
      case 2:
        //7->0
        if(!((c=mInput[mIndex++])==43)){mIndex--;goto LB_0;}
        token=21;
        goto LB_0;
      case 3:
        token=20;
        //5->6
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=16;
        //6->0
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      case 4:
        token=19;
        //3->4
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=15;
        //4->0
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=17;
        goto LB_0;
      case 5:
        token=9;
        //2->0
        if(!((c=mInput[mIndex++])==94)){mIndex--;goto LB_0;}
        token=22;
        goto LB_0;
      case 6:
        token=8;
        goto LB_0;
      case 7:
        token=13;
        goto LB_0;
      case 8:
        token=23;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region #|LITERAL|[|<|**|__|***|___|*|_|~~|++|[^|!
  protected unsafe virtual Match Match_VZZ0_spQk(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->8
    switch((c=mInput[mIndex++])<=126&&c>=33?_input__1722755199[c-33]:0)
    {
      case 1:
        //8->0
        if(!((c=mInput[mIndex++])==126)){mIndex--;goto LB_0;}
        token=7;
        goto LB_0;
      case 2:
        //7->0
        if(!((c=mInput[mIndex++])==43)){mIndex--;goto LB_0;}
        token=21;
        goto LB_0;
      case 3:
        token=20;
        //5->6
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=16;
        //6->0
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      case 4:
        token=19;
        //3->4
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=15;
        //4->0
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=17;
        goto LB_0;
      case 5:
        token=9;
        //2->0
        if(!((c=mInput[mIndex++])==94)){mIndex--;goto LB_0;}
        token=22;
        goto LB_0;
      case 6:
        token=8;
        goto LB_0;
      case 7:
        token=13;
        goto LB_0;
      case 8:
        token=23;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_spQk(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region #|[|<|**|__|***|___|*|_|~~|++|[^|!|||-|NUMBER
  protected unsafe virtual Match Match_1E58(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->9
    switch((c=mInput[mIndex++])<=126&&c>=33?_input__810804434[c-33]:0)
    {
      case 1:
        //9->0
        if(!((c=mInput[mIndex++])==126)){mIndex--;goto LB_0;}
        token=7;
        goto LB_0;
      case 2:
        //8->0
        if(!((c=mInput[mIndex++])==43)){mIndex--;goto LB_0;}
        token=21;
        goto LB_0;
      case 3:
        token=3;
        break;
      case 4:
        token=20;
        //5->6
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=16;
        //6->0
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      case 5:
        token=19;
        //3->4
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=15;
        //4->0
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=17;
        goto LB_0;
      case 6:
        token=9;
        //2->0
        if(!((c=mInput[mIndex++])==94)){mIndex--;goto LB_0;}
        token=22;
        goto LB_0;
      case 7:
        token=8;
        goto LB_0;
      case 8:
        token=13;
        goto LB_0;
      case 9:
        token=23;
        goto LB_0;
      case 10:
        token=24;
        goto LB_0;
      case 11:
        token=25;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_7:
    //7->7
    if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
    token=3;
    goto LB_7;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region #|[|<|**|__|***|___|*|_|~~|++|[^|!|||-|NUMBER|LITERAL
  protected unsafe virtual Match Match_190t_1E58(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->9
    switch((c=mInput[mIndex++])<=126&&c>=33?_input__810804434[c-33]:0)
    {
      case 1:
        //9->0
        if(!((c=mInput[mIndex++])==126)){mIndex--;goto LB_0;}
        token=7;
        goto LB_0;
      case 2:
        //8->0
        if(!((c=mInput[mIndex++])==43)){mIndex--;goto LB_0;}
        token=21;
        goto LB_0;
      case 3:
        token=3;
        break;
      case 4:
        token=20;
        //5->6
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=16;
        //6->0
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      case 5:
        token=19;
        //3->4
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=15;
        //4->0
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=17;
        goto LB_0;
      case 6:
        token=9;
        //2->0
        if(!((c=mInput[mIndex++])==94)){mIndex--;goto LB_0;}
        token=22;
        goto LB_0;
      case 7:
        token=8;
        goto LB_0;
      case 8:
        token=13;
        goto LB_0;
      case 9:
        token=23;
        goto LB_0;
      case 10:
        token=24;
        goto LB_0;
      case 11:
        token=25;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_7:
    //7->7
    if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
    token=3;
    goto LB_7;
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_1E58(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region #|LITERAL
  protected unsafe virtual Match Match_o0kw(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==35)){mIndex--;goto LB_0;}
    token=8;
    goto LB_0;
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          mIndex++;
        }
        else
        {
          break;
        }
      }
      token=2;
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region #
  protected unsafe virtual Match Match_BkUF(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==35)){mIndex--;goto LB_0;}
    token=8;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL
  protected unsafe virtual Match Match_dgl0_s1tY(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_s1tY(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL
  protected unsafe virtual Match Match_dgl0_UVNJ(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_UVNJ(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region >
  protected unsafe virtual Match Match_0Ug5(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==62)){mIndex--;goto LB_0;}
    token=14;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL
  protected unsafe virtual Match Match_dgl0_0Ug5(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_0Ug5(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL
  protected unsafe virtual Match Match_dgl0_pkMp(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_pkMp(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region __
  protected unsafe virtual Match Match_5R4N(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->2
    if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
    //2->0
    if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
    token=16;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL
  protected unsafe virtual Match Match_dgl0_5R4N(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_5R4N(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL
  protected unsafe virtual Match Match_dgl0_NkI5(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_NkI5(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region ___
  protected unsafe virtual Match Match_sRtV(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->2
    if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
    //2->3
    if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
    //3->0
    if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
    token=18;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL
  protected unsafe virtual Match Match_dgl0_sRtV(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_sRtV(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL
  protected unsafe virtual Match Match_dgl0_tMId(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_tMId(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region _
  protected unsafe virtual Match Match_pMF1(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
    token=20;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL
  protected unsafe virtual Match Match_dgl0_pMF1(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_pMF1(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL|#|[|<|**|__|***|___|*|_
  protected unsafe virtual Match Match_00Ex_UYIA(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->4
    switch((c=mInput[mIndex++])<=95&&c>=35?_input_1589240141[c-35]:0)
    {
      case 1:
        token=20;
        //4->5
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=16;
        //5->0
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      case 2:
        token=19;
        //2->3
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=15;
        //3->0
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=17;
        goto LB_0;
      case 3:
        token=8;
        goto LB_0;
      case 4:
        token=9;
        goto LB_0;
      case 5:
        token=13;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_UYIA(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL|#|[|<|**|__|***|___|*|_
  protected unsafe virtual Match Match_00Ex_Fcw1(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->4
    switch((c=mInput[mIndex++])<=95&&c>=35?_input_1589240141[c-35]:0)
    {
      case 1:
        token=20;
        //4->5
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=16;
        //5->0
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      case 2:
        token=19;
        //2->3
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=15;
        //3->0
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=17;
        goto LB_0;
      case 3:
        token=8;
        goto LB_0;
      case 4:
        token=9;
        goto LB_0;
      case 5:
        token=13;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_Fcw1(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL|~~|#|[|<|**|__|***|___|*|_|++|[^|!
  protected unsafe virtual Match Match_VZZ0_1UsZ(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->8
    switch((c=mInput[mIndex++])<=126&&c>=33?_input__1722755199[c-33]:0)
    {
      case 1:
        //8->0
        if(!((c=mInput[mIndex++])==126)){mIndex--;goto LB_0;}
        token=7;
        goto LB_0;
      case 2:
        //7->0
        if(!((c=mInput[mIndex++])==43)){mIndex--;goto LB_0;}
        token=21;
        goto LB_0;
      case 3:
        token=20;
        //5->6
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=16;
        //6->0
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      case 4:
        token=19;
        //3->4
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=15;
        //4->0
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=17;
        goto LB_0;
      case 5:
        token=9;
        //2->0
        if(!((c=mInput[mIndex++])==94)){mIndex--;goto LB_0;}
        token=22;
        goto LB_0;
      case 6:
        token=8;
        goto LB_0;
      case 7:
        token=13;
        goto LB_0;
      case 8:
        token=23;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_1UsZ(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL|~~|#|[|<|**|__|***|___|*|_|++|[^|!
  protected unsafe virtual Match Match_VZZ0(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->8
    switch((c=mInput[mIndex++])<=126&&c>=33?_input__1722755199[c-33]:0)
    {
      case 1:
        //8->0
        if(!((c=mInput[mIndex++])==126)){mIndex--;goto LB_0;}
        token=7;
        goto LB_0;
      case 2:
        //7->0
        if(!((c=mInput[mIndex++])==43)){mIndex--;goto LB_0;}
        token=21;
        goto LB_0;
      case 3:
        token=20;
        //5->6
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=16;
        //6->0
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      case 4:
        token=19;
        //3->4
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=15;
        //4->0
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=17;
        goto LB_0;
      case 5:
        token=9;
        //2->0
        if(!((c=mInput[mIndex++])==94)){mIndex--;goto LB_0;}
        token=22;
        goto LB_0;
      case 6:
        token=8;
        goto LB_0;
      case 7:
        token=13;
        goto LB_0;
      case 8:
        token=23;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          mIndex++;
        }
        else
        {
          break;
        }
      }
      token=2;
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region #|[|<|**|__|***|___|*|_
  protected unsafe virtual Match Match_MFk0(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->4
    switch((c=mInput[mIndex++])<=95&&c>=35?_input_1589240141[c-35]:0)
    {
      case 1:
        token=20;
        //4->5
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=16;
        //5->0
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      case 2:
        token=19;
        //2->3
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=15;
        //3->0
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=17;
        goto LB_0;
      case 3:
        token=8;
        goto LB_0;
      case 4:
        token=9;
        goto LB_0;
      case 5:
        token=13;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region LITERAL|NUMBER|~~|#|[|<|**|__|***|___|*|_|++|[^|!|||-
  protected unsafe virtual Match Match_190t_QBpx(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->9
    switch((c=mInput[mIndex++])<=126&&c>=33?_input__810804434[c-33]:0)
    {
      case 1:
        //9->0
        if(!((c=mInput[mIndex++])==126)){mIndex--;goto LB_0;}
        token=7;
        goto LB_0;
      case 2:
        //8->0
        if(!((c=mInput[mIndex++])==43)){mIndex--;goto LB_0;}
        token=21;
        goto LB_0;
      case 3:
        token=3;
        break;
      case 4:
        token=20;
        //5->6
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=16;
        //6->0
        if(!((c=mInput[mIndex++])==95)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      case 5:
        token=19;
        //3->4
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=15;
        //4->0
        if(!((c=mInput[mIndex++])==42)){mIndex--;goto LB_0;}
        token=17;
        goto LB_0;
      case 6:
        token=9;
        //2->0
        if(!((c=mInput[mIndex++])==94)){mIndex--;goto LB_0;}
        token=22;
        goto LB_0;
      case 7:
        token=8;
        goto LB_0;
      case 8:
        token=13;
        goto LB_0;
      case 9:
        token=23;
        goto LB_0;
      case 10:
        token=24;
        goto LB_0;
      case 11:
        token=25;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_7:
    //7->7
    if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
    token=3;
    goto LB_7;
    LB_0:
    if(token==0)
    {
      mIndex=startIndex;
      while(true)
      {
        if((c=mInput[mIndex])!=10&&c!='\0')
        {
          match=Match_QBpx(false);
          if(match!=0)
          {
            mIndex=match.SourceSpan.Start;
            token=2;
            break;
          }
          else
          {
            mIndex++;
          }
        }
        else
        {
          token=(ushort)(c=='\0'?0:2);
          break;
        }
      }
    }
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  protected unsafe virtual void Skip()
  {
    ushort token=0,c=0;
    int startIndex=mIndex;
    //1->2
    if(!(((c=mInput[mIndex++])<=32&&c>=9?_input__1525830626[c-9]:0)!=0)){mIndex--;goto LB_0;}
    token=6;
    LB_2:
    //2->2
    if(!(((c=mInput[mIndex++])<=32&&c>=9?_input__1525830626[c-9]:0)!=0)){mIndex--;goto LB_0;}
    token=6;
    goto LB_2;
    LB_0:;
    if(token!=0)
    {
      Skip();
    }
  }
  public SCTE GetMatchColor(ushort token) => __colors.ContainsKey(token)?__colors[token]:SCTE.Transparent;
  public string GetMatchName(ushort token)
  {
    switch(token)
    {
      case 2:
        return "LITERAL";
      case 3:
        return "NUMBER";
      case 6:
        return "Skip";
      case 7:
        return "~~";
      case 8:
        return "#";
      case 9:
        return "[";
      case 10:
        return "]";
      case 11:
        return "(";
      case 12:
        return ")";
      case 13:
        return "<";
      case 14:
        return ">";
      case 15:
        return "**";
      case 16:
        return "__";
      case 17:
        return "***";
      case 18:
        return "___";
      case 19:
        return "*";
      case 20:
        return "_";
      case 21:
        return "++";
      case 22:
        return "[^";
      case 23:
        return "!";
      case 24:
        return "|";
      case 25:
        return "-";
      case 26:
        return ".";
      default:
        throw new ArgumentException($"字典中不包含Token为'{token}'的令牌。");
    }
  }
  public bool IsSkipMatch(ushort token)
  {
    switch(token)
    {
      case 2:
        return false;
      case 3:
        return false;
      case 6:
        return true;
      case 7:
        return false;
      case 8:
        return false;
      case 9:
        return false;
      case 10:
        return false;
      case 11:
        return false;
      case 12:
        return false;
      case 13:
        return false;
      case 14:
        return false;
      case 15:
        return false;
      case 16:
        return false;
      case 17:
        return false;
      case 18:
        return false;
      case 19:
        return false;
      case 20:
        return false;
      case 21:
        return false;
      case 22:
        return false;
      case 23:
        return false;
      case 24:
        return false;
      case 25:
        return false;
      case 26:
        return false;
      default:
        throw new ArgumentException($"字典中不包含Token为'{token}'的令牌。");
    }
  }
  protected unsafe virtual Markdown parse(char* input,int length)
  {
    mIndex=0;
    mInput=input;
    mLength=length;
    Match readStep=default;
    Markdown result=default;
    //1->2
    result=markdown();
    //2->0
    if(!((readStep=Match_QBpx(false))==1)){ReportError(readStep.SourceSpan,$"应匹配'ε'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    goto LB_0;
    LB_0:
    return result;
  }
  protected unsafe virtual Markdown markdown(Match readStep=default)
  {
    Match loc_2_0=default;
    MarkdownItem loc_9_0=default,loc_11_0=default;
    Markdown loc_24_0=default,result=default;
    if(loc_2_0=(readStep=readStep!=0?readStep:Match_190t_1E58(false)))
    {
      switch(loc_2_0)
      {
        case 3:
        case 7:
        case 8:
        case 9:
        case 13:
        case 15:
        case 16:
        case 17:
        case 18:
        case 19:
        case 20:
        case 21:
        case 22:
        case 23:
        case 24:
        case 25:
          //3->4
          loc_9_0=markdown_item(readStep);
          result=loc_24_0=new Markdown(loc_9_0);
          break;
        case 2:
          loc_9_0=new MarkdownLiteral(mInput, loc_2_0);
          result=loc_24_0=new Markdown(loc_9_0);
          break;
        default:
          {ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
          break;
      }
    }
    else
    {
      //3->77
      result=new Markdown();
    }
    LB_4:
    switch(loc_2_0=(readStep=Match_190t_1E58(false)))
    {
      case 2:
        loc_11_0=new MarkdownLiteral(mInput, loc_2_0);
        result=loc_24_0=loc_24_0.Add(loc_11_0);
        goto LB_4;
      case 3:
      case 25:
      case 24:
      case 23:
      case 22:
      case 21:
      case 7:
      case 20:
      case 19:
      case 18:
      case 17:
      case 16:
      case 15:
      case 13:
      case 9:
      case 8:
        //4->4
        loc_11_0=markdown_item(readStep);
        result=loc_24_0=loc_24_0.Add(loc_11_0);
        goto LB_4;
      default:
        goto LB_0;
    }
    LB_0:
    return result;
  }
  protected unsafe virtual MarkdownItem markdown_item(Match readStep=default)
  {
    MarkdownItem loc_9_0=default,result=default;
    switch(readStep=readStep!=0?readStep:Match_1E58(false))
    {
      case 7:
      case 8:
      case 9:
      case 13:
      case 15:
      case 16:
      case 17:
      case 18:
      case 19:
      case 20:
      case 21:
      case 22:
      case 23:
        //5->6
        loc_9_0=simple_item(readStep);
        result=loc_9_0;
        break;
      case 3:
      case 24:
      case 25:
        //5->6
        loc_9_0=complex_item(readStep);
        result=loc_9_0;
        break;
      default:
        {ReportError(readStep.SourceSpan,$"应匹配'NUMBER'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
        break;
    }
    return result;
  }
  protected unsafe virtual MarkdownItem complex_item(Match readStep=default)
  {
    Table loc_15_0=default;
    UnorderedList loc_18_0=default;
    OrderedList loc_22_0=default;
    MarkdownItem result=default;
    switch(readStep=readStep!=0?readStep:Match_QNNh(false))
    {
      case 3:
        //7->8
        loc_22_0=ordered_list(readStep);
        result=loc_22_0;
        break;
      case 25:
        //7->8
        loc_18_0=unordered_list(readStep);
        result=loc_18_0;
        break;
      case 24:
        //7->8
        loc_15_0=table(readStep);
        result=loc_15_0;
        break;
      default:
        {ReportError(readStep.SourceSpan,$"应匹配'|'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
        break;
    }
    return result;
  }
  protected unsafe virtual OrderedList ordered_list(Match readStep=default)
  {
    OrderedListItem loc_21_0=default,loc_23_0=default;
    OrderedList loc_22_0=default,result=default;
    //9->10
    loc_21_0=ordered_list_item(readStep);
    result=loc_22_0=new OrderedList(loc_21_0);
    LB_10:
    if((readStep=Match_8QYB(false))==3)
    {
      //10->10
      loc_23_0=ordered_list_item(readStep);
      result=loc_22_0.Add(loc_23_0);
      goto LB_10;
    }
    else
    {
      goto LB_0;
    }
    LB_0:
    return result;
  }
  protected unsafe virtual OrderedListItem ordered_list_item(Match readStep=default)
  {
    Match loc_2_1=default,loc_2_0=default;
    MarkdownItem loc_9_0=default,loc_20_0=default;
    OrderedListItem result=default;
    //11->12
    if(!((loc_2_0=(readStep=readStep!=0?readStep:Match_8QYB(false)))==3)){ReportError(readStep.SourceSpan,$"应匹配'NUMBER'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    //12->13
    if(!((readStep=Match_tMkQ(false))==26)){ReportError(readStep.SourceSpan,$"应匹配'.'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    switch(loc_2_1=(readStep=Match_VZZ0(false)))
    {
      case 7:
      case 8:
      case 9:
      case 13:
      case 15:
      case 16:
      case 17:
      case 18:
      case 19:
      case 20:
      case 21:
      case 22:
      case 23:
        //13->14
        loc_20_0=simple_item(readStep);
        result=new OrderedListItem(loc_2_0, loc_20_0);
        break;
      case 2:
        loc_9_0=new MarkdownLiteral(mInput, loc_2_1);
        loc_20_0=loc_9_0;
        result=new OrderedListItem(loc_2_0, loc_20_0);
        break;
      default:
        {ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
        break;
    }
    return result;
  }
  protected unsafe virtual MarkdownItem simple_item(Match readStep=default)
  {
    MarkdownItem loc_9_0=default,result=default;
    switch(readStep=readStep!=0?readStep:Match_spQk(false))
    {
      case 8:
      case 9:
      case 13:
      case 15:
      case 16:
      case 17:
      case 18:
      case 19:
      case 20:
        //15->16
        loc_9_0=prim(readStep);
        result=loc_9_0;
        break;
      case 23:
        //15->16
        loc_9_0=image(readStep);
        result=loc_9_0;
        break;
      case 22:
        //15->16
        loc_9_0=label(readStep);
        result=loc_9_0;
        break;
      case 21:
        //15->16
        loc_9_0=underline(readStep);
        result=loc_9_0;
        break;
      case 7:
        //15->16
        loc_9_0=delete(readStep);
        result=loc_9_0;
        break;
      default:
        {ReportError(readStep.SourceSpan,$"应匹配'~~'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
        break;
    }
    return result;
  }
  protected unsafe virtual MarkdownItem prim(Match readStep=default)
  {
    NumberTitle loc_4_0=default;
    Link loc_8_0=default;
    MarkdownItem loc_9_0=default,result=default;
    switch(readStep=readStep!=0?readStep:Match_MFk0(false))
    {
      case 19:
      case 20:
        //17->18
        loc_9_0=italics(readStep);
        result=loc_9_0;
        break;
      case 17:
      case 18:
        //17->18
        loc_9_0=italicsbold(readStep);
        result=loc_9_0;
        break;
      case 15:
      case 16:
        //17->18
        loc_9_0=bold(readStep);
        result=loc_9_0;
        break;
      case 9:
      case 13:
        //17->18
        loc_8_0=link(readStep);
        result=loc_8_0;
        break;
      case 8:
        //17->18
        loc_4_0=numtitle(readStep);
        result=loc_4_0;
        break;
      default:
        {ReportError(readStep.SourceSpan,$"应匹配'#'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
        break;
    }
    return result;
  }
  protected unsafe virtual MarkdownItem italics(Match readStep=default)
  {
    Match loc_2_1=default,loc_2_0=default,loc_7_0=default;
    MarkdownLiteral loc_3_0=default;
    MarkdownItem result=default;
    //19->20
    if((loc_2_0=(readStep=readStep!=0?readStep:Match_9Ftd(false)))==20)
    {
      //20->21
      if(!((loc_2_1=(readStep=Match_dgl0_pMF1(false)))==2)){ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      loc_3_0=new MarkdownLiteral(mInput, loc_2_1);
      //21->22
      if(!((loc_7_0=(readStep=Match_pMF1(false)))==20)){ReportError(readStep.SourceSpan,$"应匹配'_'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      result=new Italics(loc_3_0, loc_2_0, loc_7_0);
    }
    else if(loc_2_0==19)
    {
      //23->24
      if(!((loc_2_1=(readStep=Match_dgl0_tMId(false)))==2)){ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      loc_3_0=new MarkdownLiteral(mInput, loc_2_1);
      //24->22
      if(!((loc_7_0=(readStep=Match_tMId(false)))==19)){ReportError(readStep.SourceSpan,$"应匹配'*'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      result=new Italics(loc_3_0, loc_2_0, loc_7_0);
    }
    return result;
  }
  protected unsafe virtual MarkdownItem italicsbold(Match readStep=default)
  {
    Match loc_2_1=default,loc_2_0=default,loc_7_0=default;
    MarkdownLiteral loc_3_0=default;
    MarkdownItem result=default;
    //25->26
    if((loc_2_0=(readStep=readStep!=0?readStep:Match_dA5h(false)))==18)
    {
      //26->27
      if(!((loc_2_1=(readStep=Match_dgl0_sRtV(false)))==2)){ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      loc_3_0=new MarkdownLiteral(mInput, loc_2_1);
      //27->28
      if(!((loc_7_0=(readStep=Match_sRtV(false)))==18)){ReportError(readStep.SourceSpan,$"应匹配'___'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      result=new ItalicsBold(loc_3_0, loc_2_0, loc_7_0);
    }
    else if(loc_2_0==17)
    {
      //29->30
      if(!((loc_2_1=(readStep=Match_dgl0_NkI5(false)))==2)){ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      loc_3_0=new MarkdownLiteral(mInput, loc_2_1);
      //30->28
      if(!((loc_7_0=(readStep=Match_NkI5(false)))==17)){ReportError(readStep.SourceSpan,$"应匹配'***'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      result=new ItalicsBold(loc_3_0, loc_2_0, loc_7_0);
    }
    return result;
  }
  protected unsafe virtual MarkdownItem bold(Match readStep=default)
  {
    Match loc_2_1=default,loc_2_0=default,loc_7_0=default;
    MarkdownLiteral loc_3_0=default;
    MarkdownItem result=default;
    //31->32
    if((loc_2_0=(readStep=readStep!=0?readStep:Match_15gk(false)))==16)
    {
      //32->33
      if(!((loc_2_1=(readStep=Match_dgl0_5R4N(false)))==2)){ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      loc_3_0=new MarkdownLiteral(mInput, loc_2_1);
      //33->34
      if(!((loc_7_0=(readStep=Match_5R4N(false)))==16)){ReportError(readStep.SourceSpan,$"应匹配'__'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      result=new Bold(loc_3_0, loc_2_0, loc_7_0);
    }
    else if(loc_2_0==15)
    {
      //35->36
      if(!((loc_2_1=(readStep=Match_dgl0_pkMp(false)))==2)){ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      loc_3_0=new MarkdownLiteral(mInput, loc_2_1);
      //36->34
      if(!((loc_7_0=(readStep=Match_pkMp(false)))==15)){ReportError(readStep.SourceSpan,$"应匹配'**'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      result=new Bold(loc_3_0, loc_2_0, loc_7_0);
    }
    return result;
  }
  protected unsafe virtual Link link(Match readStep=default)
  {
    Match loc_2_1=default,loc_2_0=default,loc_7_0=default,loc_6_0=default;
    MarkdownLiteral loc_3_0=default,loc_5_0=default;
    Link result=default;
    //37->38
    if((loc_2_0=(readStep=readStep!=0?readStep:Match_U4Zk(false)))==13)
    {
      //38->39
      if(!((loc_2_1=(readStep=Match_dgl0_0Ug5(false)))==2)){ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      loc_3_0=new MarkdownLiteral(mInput, loc_2_1);
      //39->40
      if(!((loc_7_0=(readStep=Match_0Ug5(false)))==14)){ReportError(readStep.SourceSpan,$"应匹配'>'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      result=new Link(loc_3_0, loc_2_0, loc_7_0);
    }
    else if(loc_2_0==9)
    {
      //41->42
      if(!((loc_2_1=(readStep=Match_dgl0_s1tY(false)))==2)){ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      loc_3_0=new MarkdownLiteral(mInput, loc_2_1);
      //42->43
      if(!((readStep=Match_s1tY(false))==10)){ReportError(readStep.SourceSpan,$"应匹配']'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      //43->44
      if(!((readStep=Match_ZBkV(false))==11)){ReportError(readStep.SourceSpan,$"应匹配'('时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      //44->45
      if(!((loc_2_1=(readStep=Match_dgl0_UVNJ(false)))==2)){ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      loc_5_0=new MarkdownLiteral(mInput, loc_2_1);
      //45->40
      if(!((loc_6_0=(readStep=Match_UVNJ(false)))==12)){ReportError(readStep.SourceSpan,$"应匹配')'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
      result=new Link(loc_3_0, loc_5_0, loc_2_0, loc_6_0);
    }
    return result;
  }
  protected unsafe virtual NumberTitle numtitle(Match readStep=default)
  {
    Match loc_2_1=default,loc_2_0=default;
    MarkdownLiteral loc_3_0=default;
    NumberTitle loc_1_0=default,result=default;
    //46->47
    if(!((loc_2_0=(readStep=readStep!=0?readStep:Match_BkUF(false)))==8)){ReportError(readStep.SourceSpan,$"应匹配'#'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    if((loc_2_1=(readStep=Match_o0kw(false)))==2)
    {
      loc_3_0=new MarkdownLiteral(mInput, loc_2_1);
      result=new NumberTitle(loc_2_0, loc_3_0);
    }
    else if(loc_2_1==8)
    {
      //47->48
      loc_1_0=numtitle(readStep);
      result=loc_1_0.AddLevel(loc_2_0);
    }
    return result;
  }
  protected unsafe virtual MarkdownItem image(Match readStep=default)
  {
    Link loc_12_0=default;
    Match loc_2_0=default;
    MarkdownItem result=default;
    //49->50
    if(!((loc_2_0=(readStep=readStep!=0?readStep:Match_4Fpo(false)))==23)){ReportError(readStep.SourceSpan,$"应匹配'!'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    //50->51
    loc_12_0=link();
    result=new MarkdownImage(loc_12_0, loc_2_0);
    return result;
  }
  protected unsafe virtual MarkdownItem label(Match readStep=default)
  {
    Match loc_2_1=default,loc_2_0=default,loc_7_0=default;
    MarkdownLiteral loc_3_0=default;
    MarkdownItem result=default;
    //52->53
    if(!((loc_2_0=(readStep=readStep!=0?readStep:Match_lctR(false)))==22)){ReportError(readStep.SourceSpan,$"应匹配'[^'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    //53->54
    if(!((loc_2_1=(readStep=Match_dgl0_s1tY(false)))==2)){ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    loc_3_0=new MarkdownLiteral(mInput, loc_2_1);
    //54->55
    if(!((loc_7_0=(readStep=Match_s1tY(false)))==10)){ReportError(readStep.SourceSpan,$"应匹配']'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    result=new MarkdownLabel(loc_3_0, loc_2_0, loc_7_0);
    return result;
  }
  protected unsafe virtual MarkdownItem underline(Match readStep=default)
  {
    Match loc_2_1=default,loc_2_0=default,loc_7_0=default;
    MarkdownItem loc_11_0=default,result=default;
    //56->57
    if(!((loc_2_0=(readStep=readStep!=0?readStep:Match_Fcw1(false)))==21)){ReportError(readStep.SourceSpan,$"应匹配'++'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    switch(loc_2_1=(readStep=Match_00Ex_Fcw1(false)))
    {
      case 8:
      case 9:
      case 13:
      case 15:
      case 16:
      case 17:
      case 18:
      case 19:
      case 20:
        //57->58
        loc_11_0=prim(readStep);
        break;
      case 2:
        loc_11_0=new MarkdownLiteral(mInput, loc_2_1);
        break;
      default:
        {ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
        break;
    }
    //58->59
    if(!((loc_7_0=(readStep=Match_Fcw1(false)))==21)){ReportError(readStep.SourceSpan,$"应匹配'++'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    result=new Underline(loc_11_0, loc_2_0, loc_7_0);
    return result;
  }
  protected unsafe virtual MarkdownItem delete(Match readStep=default)
  {
    Match loc_2_1=default,loc_2_0=default,loc_7_0=default;
    MarkdownItem loc_11_0=default,result=default;
    //60->61
    if(!((loc_2_0=(readStep=readStep!=0?readStep:Match_UYIA(false)))==7)){ReportError(readStep.SourceSpan,$"应匹配'~~'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    switch(loc_2_1=(readStep=Match_00Ex_UYIA(false)))
    {
      case 8:
      case 9:
      case 13:
      case 15:
      case 16:
      case 17:
      case 18:
      case 19:
      case 20:
        //61->62
        loc_11_0=prim(readStep);
        break;
      case 2:
        loc_11_0=new MarkdownLiteral(mInput, loc_2_1);
        break;
      default:
        {ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
        break;
    }
    //62->63
    if(!((loc_7_0=(readStep=Match_UYIA(false)))==7)){ReportError(readStep.SourceSpan,$"应匹配'~~'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    result=new Delete(loc_11_0, loc_2_0, loc_7_0);
    return result;
  }
  protected unsafe virtual UnorderedList unordered_list(Match readStep=default)
  {
    UnorderedListItem loc_17_0=default,loc_19_0=default;
    UnorderedList loc_18_0=default,result=default;
    //64->65
    loc_17_0=unordered_list_item(readStep);
    result=loc_18_0=new UnorderedList(loc_17_0);
    LB_65:
    if((readStep=Match_Vgwl(false))==25)
    {
      //65->65
      loc_19_0=unordered_list_item(readStep);
      result=loc_18_0.Add(loc_19_0);
      goto LB_65;
    }
    else
    {
      goto LB_0;
    }
    LB_0:
    return result;
  }
  protected unsafe virtual UnorderedListItem unordered_list_item(Match readStep=default)
  {
    Match loc_2_1=default,loc_2_0=default;
    MarkdownItem loc_9_0=default,loc_11_0=default;
    UnorderedListItem result=default;
    //66->67
    if(!((loc_2_0=(readStep=readStep!=0?readStep:Match_Vgwl(false)))==25)){ReportError(readStep.SourceSpan,$"应匹配'-'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    switch(loc_2_1=(readStep=Match_VZZ0(false)))
    {
      case 7:
      case 8:
      case 9:
      case 13:
      case 15:
      case 16:
      case 17:
      case 18:
      case 19:
      case 20:
      case 21:
      case 22:
      case 23:
        //67->68
        loc_11_0=simple_item(readStep);
        result=new UnorderedListItem(loc_2_0, loc_11_0);
        break;
      case 2:
        loc_9_0=new MarkdownLiteral(mInput, loc_2_1);
        loc_11_0=loc_9_0;
        result=new UnorderedListItem(loc_2_0, loc_11_0);
        break;
      default:
        {ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
        break;
    }
    return result;
  }
  protected unsafe virtual Table table(Match readStep=default)
  {
    TableRow loc_14_0=default,loc_16_0=default;
    Table loc_15_0=default,result=default;
    //69->70
    loc_14_0=table_row(readStep);
    result=loc_15_0=new Table(loc_14_0);
    LB_70:
    if((readStep=Match_1UsZ(false))==24)
    {
      //70->70
      loc_16_0=table_row(readStep);
      result=loc_15_0.Add(loc_16_0);
      goto LB_70;
    }
    else
    {
      goto LB_0;
    }
    LB_0:
    return result;
  }
  protected unsafe virtual TableRow table_row(Match readStep=default)
  {
    TableRowItem loc_13_0=default;
    TableRow loc_14_0=default,result=default;
    //71->72
    if(!((readStep=readStep!=0?readStep:Match_1UsZ(false))==24)){ReportError(readStep.SourceSpan,$"应匹配'|'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    //72->73
    loc_13_0=table_row_item();
    result=loc_14_0=new TableRow(loc_13_0);
    LB_73:
    switch(readStep=Match_VZZ0_spQk(false))
    {
      case 23:
      case 22:
      case 21:
      case 7:
      case 20:
      case 19:
      case 18:
      case 17:
      case 16:
      case 15:
      case 13:
      case 9:
      case 2:
      case 8:
        //73->73
        loc_13_0=table_row_item(readStep);
        result=loc_14_0.Add(loc_13_0);
        goto LB_73;
      default:
        goto LB_0;
    }
    LB_0:
    return result;
  }
  protected unsafe virtual TableRowItem table_row_item(Match readStep=default)
  {
    Match loc_2_0=default;
    MarkdownItem loc_9_0=default;
    TableRowItem result=default;
    switch(loc_2_0=(readStep=readStep!=0?readStep:Match_VZZ0_1UsZ(false)))
    {
      case 7:
      case 8:
      case 9:
      case 13:
      case 15:
      case 16:
      case 17:
      case 18:
      case 19:
      case 20:
      case 21:
      case 22:
      case 23:
        //74->75
        loc_9_0=simple_item(readStep);
        break;
      case 2:
        loc_9_0=new MarkdownLiteral(mInput, loc_2_0);
        break;
      default:
        {ReportError(readStep.SourceSpan,$"应匹配'LITERAL'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
        break;
    }
    //75->76
    if(!((readStep=Match_1UsZ(false))==24)){ReportError(readStep.SourceSpan,$"应匹配'|'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    result=new TableRowItem(loc_9_0);
    return result;
  }
}