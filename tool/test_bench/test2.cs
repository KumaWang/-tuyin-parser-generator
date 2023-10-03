using System;
using SCTE=System.Drawing.Color;
using Tuitor.packages.richtext.format;
using Tuitor.packages.richtext.format.parsers.markdown;
namespace Tuitor.packages.richtext.format.parsers;
partial class MarkdownParser
{
  private unsafe char* mInput;
  private int mLength;
  private int mIndex;
  private byte[] _input_1060633704=new byte[]{1,0,2};
  private byte[] _input__2035455145=new byte[]{2,0,1};
  private byte[] _input__1073157673=new byte[]{1,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1};
  private readonly Dictionary<ushort,SCTE> __colors=new(){};
  #region >>
  protected unsafe virtual Match Match_8hEF(bool close,ParseReport formal_0)
  {
    if(!close)Skip(formal_0);
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->2
    if(!((c=mInput[mIndex++])==62)){mIndex--;goto LB_0;}
    //2->0
    if(!((c=mInput[mIndex++])==62)){mIndex--;goto LB_0;}
    token=7;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    if(token!=0)
    {
      OnMatch(startIndex,mIndex,token,formal_0);
    }
    return match;
  }
  #endregion
  #region +
  protected unsafe virtual Match Match_sck8(bool close,ParseReport formal_0)
  {
    if(!close)Skip(formal_0);
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==43)){mIndex--;goto LB_0;}
    token=8;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    if(token!=0)
    {
      OnMatch(startIndex,mIndex,token,formal_0);
    }
    return match;
  }
  #endregion
  #region 1
  protected unsafe virtual Match Match_FBY8(bool close,ParseReport formal_0)
  {
    if(!close)Skip(formal_0);
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==49)){mIndex--;goto LB_0;}
    token=9;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    if(token!=0)
    {
      OnMatch(startIndex,mIndex,token,formal_0);
    }
    return match;
  }
  #endregion
  #region -
  protected unsafe virtual Match Match_tcUE(bool close,ParseReport formal_0)
  {
    if(!close)Skip(formal_0);
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==45)){mIndex--;goto LB_0;}
    token=10;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    if(token!=0)
    {
      OnMatch(startIndex,mIndex,token,formal_0);
    }
    return match;
  }
  #endregion
  #region <<
  protected unsafe virtual Match Match_t4Vc(bool close,ParseReport formal_0)
  {
    if(!close)Skip(formal_0);
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->2
    if(!((c=mInput[mIndex++])==60)){mIndex--;goto LB_0;}
    //2->0
    if(!((c=mInput[mIndex++])==60)){mIndex--;goto LB_0;}
    token=11;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    if(token!=0)
    {
      OnMatch(startIndex,mIndex,token,formal_0);
    }
    return match;
  }
  #endregion
  #region ε
  protected unsafe virtual Match Match_EIB8(bool close,ParseReport formal_0)
  {
    if(!close)Skip(formal_0);
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==0)){mIndex--;goto LB_0;}
    token=1;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    if(token!=0)
    {
      OnMatch(startIndex,mIndex,token,formal_0);
    }
    return match;
  }
  #endregion
  #region +|-
  protected unsafe virtual Match Match_t194(bool close,ParseReport formal_0)
  {
    if(!close)Skip(formal_0);
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    switch((c=mInput[mIndex++])<=45&&c>=43?_input_1060633704[c-43]:0)
    {
      case 1:
        token=8;
        goto LB_0;
      case 2:
        token=10;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    match=new Match(token,startIndex,mIndex);
    if(token!=0)
    {
      OnMatch(startIndex,mIndex,token,formal_0);
    }
    return match;
  }
  #endregion
  #region >>|<<
  protected unsafe virtual Match Match_RoAw(bool close,ParseReport formal_0)
  {
    if(!close)Skip(formal_0);
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->3
    switch((c=mInput[mIndex++])<=62&&c>=60?_input__2035455145[c-60]:0)
    {
      case 1:
        //3->0
        if(!((c=mInput[mIndex++])==62)){mIndex--;goto LB_0;}
        token=7;
        goto LB_0;
      case 2:
        //2->0
        if(!((c=mInput[mIndex++])==60)){mIndex--;goto LB_0;}
        token=11;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    match=new Match(token,startIndex,mIndex);
    if(token!=0)
    {
      OnMatch(startIndex,mIndex,token,formal_0);
    }
    return match;
  }
  #endregion
  protected unsafe virtual void Skip(ParseReport formal_0)
  {
    ushort token=0,c=0;
    int startIndex=mIndex;
    var lastState=GetState(mIndex,formal_0);
    State(mIndex,0,formal_0);
    //1->2
    if(!(((c=mInput[mIndex++])<=32&&c>=9?_input__1073157673[c-9]:0)!=0)){mIndex--;goto LB_0;}
    token=6;
    LB_2:
    //2->2
    if(!(((c=mInput[mIndex++])<=32&&c>=9?_input__1073157673[c-9]:0)!=0)){mIndex--;goto LB_0;}
    token=6;
    goto LB_2;
    LB_0:
    State(startIndex,lastState,formal_0);
    if(token!=0)
    {
      OnMatch(startIndex,mIndex,token,formal_0);
    }
    if(token!=0)
    {
      Skip(formal_0);
    }
  }
  public SCTE GetMatchColor(ushort token) => __colors.ContainsKey(token)?__colors[token]:SCTE.Transparent;
  public string GetMatchName(ushort token)
  {
    switch(token)
    {
      case 6:
        return "Skip";
      case 7:
        return ">>";
      case 8:
        return "+";
      case 9:
        return "1";
      case 10:
        return "-";
      case 11:
        return "<<";
      default:
        throw new ArgumentException($"字典中不包含Token为'{token}'的令牌。");
    }
  }
  public bool IsSkipMatch(ushort token)
  {
    switch(token)
    {
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
      default:
        throw new ArgumentException($"字典中不包含Token为'{token}'的令牌。");
    }
  }
  public (string Literal,string Snippet)[] GetFollowWords(ushort state)
  {
    switch(state)
    {
      case 18:
        return new (string,string)[] {(">>",null)};
      case 16:
        return new (string,string)[] {("+",null)};
      case 13:
      case 1:
      case 17:
      case 6:
      case 15:
      case 11:
      case 8:
      case 3:
        return new (string,string)[] {("1",null)};
      case 12:
      case 9:
        return new (string,string)[] {("-",null)};
      case 7:
      case 4:
        return new (string,string)[] {("<<",null)};
      default:
        return new (string,string)[0];
    }
  }
  protected unsafe virtual ExprRoot parse(char* input,int length,ParseReport formal_0)
  {
    mIndex=0;
    mInput=input;
    mLength=length;
    Match readStep=default;
    ExprRoot result=default;
    State(mIndex,1,formal_0);
    //1->2
    result=exp_shift(formal_0);
    State(mIndex,2,formal_0);
    //2->0
    if(!((readStep=Match_EIB8(false,formal_0))==1)){ReportError(readStep.SourceSpan,$"应匹配'ε'时扫描到了一个'{GetMatchName(readStep.Token)}'。",formal_0);}
    goto LB_0;
    LB_0:
    return result;
  }
  protected unsafe virtual ExprRoot exp_shift(ParseReport formal_0,Match readStep=default)
  {
    ExprRoot loc_2_0=default,loc_3_0=default,result=default;
    State(mIndex,3,formal_0);
    //3->4
    result=loc_2_0=exp_as(formal_0,readStep);
    if((readStep=Match_RoAw(false,formal_0))==11)
    {
    }
    else if(readStep==7)
    {
      goto LB_17;
    }
    LB_6:
    if((readStep=Match_FBY8(false,formal_0))==9)
    {
      State(mIndex,6,formal_0);
      //6->7
      loc_3_0=exp_as(formal_0,readStep);
      result=new ExprShiftLeft(loc_2_0, loc_3_0);
      if((readStep=Match_t4Vc(false,formal_0))==11)
      {
        goto LB_6;
      }
    }
    else
    {
      goto LB_0;
    }
    LB_17:
    if((readStep=Match_FBY8(false,formal_0))==9)
    {
      State(mIndex,17,formal_0);
      //17->18
      loc_3_0=exp_as(formal_0,readStep);
      result=new ExprShiftRight(loc_2_0, loc_3_0);
      if((readStep=Match_8hEF(false,formal_0))==7)
      {
        goto LB_17;
      }
    }
    else
    {
      goto LB_0;
    }
    LB_0:
    return result;
  }
  protected unsafe virtual ExprRoot exp_as(ParseReport formal_0,Match readStep=default)
  {
    ExprRoot loc_2_0=default,loc_3_0=default,result=default;
    State(mIndex,8,formal_0);
    //8->9
    result=loc_2_0=exp_mdr(formal_0,readStep);
    if((readStep=Match_t194(false,formal_0))==10)
    {
    }
    else if(readStep==8)
    {
      goto LB_15;
    }
    LB_11:
    if((readStep=Match_FBY8(false,formal_0))==9)
    {
      State(mIndex,11,formal_0);
      //11->12
      loc_3_0=exp_mdr(formal_0,readStep);
      result=new ExprSub(loc_2_0, loc_3_0);
      if((readStep=Match_tcUE(false,formal_0))==10)
      {
        goto LB_11;
      }
    }
    else
    {
      goto LB_0;
    }
    LB_15:
    if((readStep=Match_FBY8(false,formal_0))==9)
    {
      State(mIndex,15,formal_0);
      //15->16
      loc_3_0=exp_mdr(formal_0,readStep);
      result=new ExprAdd(loc_2_0, loc_3_0);
      if((readStep=Match_sck8(false,formal_0))==8)
      {
        goto LB_15;
      }
    }
    else
    {
      goto LB_0;
    }
    LB_0:
    return result;
  }
  protected unsafe virtual ExprRoot exp_mdr(ParseReport formal_0,Match readStep=default)
  {
    Match loc_1_0=default;
    ExprRoot result=default;
    State(mIndex,13,formal_0);
    //13->14
    if(!((loc_1_0=(readStep=readStep!=0?readStep:Match_FBY8(false,formal_0)))==9)){ReportError(readStep.SourceSpan,$"应匹配'1'时扫描到了一个'{GetMatchName(readStep.Token)}'。",formal_0);}
    result=loc_1_0;
    return result;
  }
}