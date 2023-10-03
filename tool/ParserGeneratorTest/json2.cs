using System;
using SCTE=System.Drawing.Color;
using Tuitor.packages.richtext.format;
namespace ParserGeneratorTest;
partial class JsonParser
{
  private unsafe char* mInput;
  private int mLength;
  private int mIndex;
  private byte[] _input__568209659=new byte[]{0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,3,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2};
  private byte[] _input__1383441171=new byte[]{0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0};
  private byte[] _input__786325737=new byte[]{5,0,0,0,0,0,0,0,0,0,0,8,0,0,6,7,7,7,7,7,7,7,7,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,1,0,0,0,0,0,2};
  private byte[] _input__621769226=new byte[]{1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1};
  private byte[] _input__708474162=new byte[]{1,0,0,2,2,2,2,2,2,2,2,2,2};
  private byte[] _input_1040367878=new byte[]{2,0,1,1,1,1,1,1,1,1,1,1};
  private byte[] _input_1848973207=new byte[]{3,0,4,4,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1};
  private byte[] _input__1471391470=new byte[]{2,2,2,2,2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1};
  private byte[] _input_1061576110=new byte[]{2,0,3,3,3,3,3,3,3,3,3,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1};
  private byte[] _input__1448850722=new byte[]{2,0,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2};
  private byte[] _input__1332063121=new byte[]{5,0,0,0,0,0,0,0,0,0,0,8,0,0,6,7,7,7,7,7,7,7,7,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,10,4,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,1,0,0,0,0,0,2,0,0,0,0,0,0,9};
  private byte[] _input_1618766050=new byte[]{1,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2};
  private byte[] _input__37859633=new byte[]{1,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1};
  private byte[] _input_726480692=new byte[]{0,1,1,1,1,1,1,1,1,1,0};
  private readonly Dictionary<ushort,SCTE> __colors=new(){{11,SCTE.FromArgb(255,87,166,74)}};
  #region {
  protected unsafe virtual Match Match_cwtp(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==123)){mIndex--;goto LB_0;}
    token=12;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region STRING
  protected unsafe virtual Match Match_NR1Q(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->2
    if(!((c=mInput[mIndex++])==34)){mIndex--;goto LB_0;}
    LB_2:
    //2->2
    switch((c=mInput[mIndex++])<=92?_input__568209659[c]:1)
    {
      case 1:
        goto LB_2;
      case 2:
        break;
      case 3:
        token=9;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_3:
    //3->2
    switch((c=mInput[mIndex++])<=92?_input__1383441171[c]:1)
    {
      case 1:
        goto LB_2;
      case 2:
        token=9;
        //4->2
        switch((c=mInput[mIndex++])<=92?_input__568209659[c]:1)
        {
          case 1:
            goto LB_2;
          case 2:
            goto LB_3;
          case 3:
            token=9;
            goto LB_0;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region :
  protected unsafe virtual Match Match_NN1c(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==58)){mIndex--;goto LB_0;}
    token=13;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region ,
  protected unsafe virtual Match Match_0MEZ(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==44)){mIndex--;goto LB_0;}
    token=14;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region }
  protected unsafe virtual Match Match_VhQc(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==125)){mIndex--;goto LB_0;}
    token=15;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region [
  protected unsafe virtual Match Match_gRxt(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==91)){mIndex--;goto LB_0;}
    token=16;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region ]
  protected unsafe virtual Match Match_Q9El(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->0
    if(!((c=mInput[mIndex++])==93)){mIndex--;goto LB_0;}
    token=17;
    goto LB_0;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region null|true|false|NUMBER|SCIENTIFIC|UNICODE|HEX|STRING
  protected unsafe virtual Match Match_0xEU(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->46
    switch((c=mInput[mIndex++])<=116&&c>=34?_input__786325737[c-34]:0)
    {
      case 1:
        //46->47
        if(!((c=mInput[mIndex++])==117)){mIndex--;goto LB_0;}
        //47->48
        if(!((c=mInput[mIndex++])==108)){mIndex--;goto LB_0;}
        //48->0
        if(!((c=mInput[mIndex++])==108)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      case 2:
        //43->44
        if(!((c=mInput[mIndex++])==114)){mIndex--;goto LB_0;}
        //44->45
        if(!((c=mInput[mIndex++])==117)){mIndex--;goto LB_0;}
        //45->0
        if(!((c=mInput[mIndex++])==101)){mIndex--;goto LB_0;}
        token=19;
        goto LB_0;
      case 3:
        //39->40
        if(!((c=mInput[mIndex++])==97)){mIndex--;goto LB_0;}
        //40->41
        if(!((c=mInput[mIndex++])==108)){mIndex--;goto LB_0;}
        //41->42
        if(!((c=mInput[mIndex++])==115)){mIndex--;goto LB_0;}
        //42->0
        if(!((c=mInput[mIndex++])==101)){mIndex--;goto LB_0;}
        token=20;
        goto LB_0;
      case 4:
        //34->35
        if(!((c=mInput[mIndex++])==117)){mIndex--;goto LB_0;}
        //35->36
        if(!(((c=mInput[mIndex++])<=122&&c>=48?_input__621769226[c-48]:0)!=0)){mIndex--;goto LB_0;}
        //36->37
        if(!(((c=mInput[mIndex++])<=122&&c>=48?_input__621769226[c-48]:0)!=0)){mIndex--;goto LB_0;}
        //37->38
        if(!(((c=mInput[mIndex++])<=122&&c>=48?_input__621769226[c-48]:0)!=0)){mIndex--;goto LB_0;}
        //38->0
        if(!(((c=mInput[mIndex++])<=122&&c>=48?_input__621769226[c-48]:0)!=0)){mIndex--;goto LB_0;}
        token=5;
        goto LB_0;
      case 5:
        break;
      case 6:
        token=2;
        //27->29
        switch((c=mInput[mIndex++])<=120&&c>=46?_input_1848973207[c-46]:0)
        {
          case 1:
            //29->30
            if(!(((c=mInput[mIndex++])<=122&&c>=48?_input__621769226[c-48]:0)!=0)){mIndex--;goto LB_0;}
            //30->0
            if(!(((c=mInput[mIndex++])<=122&&c>=48?_input__621769226[c-48]:0)!=0)){mIndex--;goto LB_0;}
            token=6;
            goto LB_0;
          case 2:
            goto LB_14;
          case 3:
            //28->10
            if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
            token=2;
            goto LB_10;
          case 4:
            token=2;
            goto LB_5;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      case 7:
        token=2;
        //22->14
        switch((c=mInput[mIndex++])<=101&&c>=46?_input_1061576110[c-46]:0)
        {
          case 1:
            goto LB_14;
          case 2:
            //25->26
            if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
            token=2;
            //26->10
            if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
            token=2;
            goto LB_10;
          case 3:
            token=2;
            //23->24
            switch((c=mInput[mIndex++])<=101&&c>=46?_input__1448850722[c-46]:0)
            {
              case 1:
                token=2;
                //24->5
                switch((c=mInput[mIndex++])<=101&&c>=46?_input__1448850722[c-46]:0)
                {
                  case 1:
                    token=2;
                    goto LB_5;
                  case 2:
                    goto LB_5;
                  default:
                    {mIndex--;goto LB_0;}
                    break;
                }
                break;
              case 2:
                goto LB_5;
              default:
                {mIndex--;goto LB_0;}
                break;
            }
            break;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      case 8:
        //2->3
        if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
        token=2;
        //3->14
        switch((c=mInput[mIndex++])<=101&&c>=46?_input_1061576110[c-46]:0)
        {
          case 1:
            goto LB_14;
          case 2:
            //20->21
            if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
            token=2;
            //21->10
            if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
            token=2;
            goto LB_10;
          case 3:
            token=2;
            //4->19
            switch((c=mInput[mIndex++])<=101&&c>=46?_input__1448850722[c-46]:0)
            {
              case 1:
                token=2;
                //19->5
                switch((c=mInput[mIndex++])<=101&&c>=46?_input__1448850722[c-46]:0)
                {
                  case 1:
                    token=2;
                    goto LB_5;
                  case 2:
                    goto LB_5;
                  default:
                    {mIndex--;goto LB_0;}
                    break;
                }
                break;
              case 2:
                goto LB_5;
              default:
                {mIndex--;goto LB_0;}
                break;
            }
            break;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_31:
    //31->31
    switch((c=mInput[mIndex++])<=92?_input__568209659[c]:1)
    {
      case 1:
        goto LB_31;
      case 2:
        goto LB_32;
      case 3:
        token=9;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_5:
    //5->14
    switch((c=mInput[mIndex++])<=101&&c>=46?_input_1061576110[c-46]:0)
    {
      case 1:
        goto LB_14;
      case 2:
        //8->9
        if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
        token=2;
        //9->10
        if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
        token=2;
        break;
      case 3:
        token=2;
        //6->7
        switch((c=mInput[mIndex++])<=101&&c>=46?_input__1448850722[c-46]:0)
        {
          case 1:
            token=2;
            //7->5
            switch((c=mInput[mIndex++])<=101&&c>=46?_input__1448850722[c-46]:0)
            {
              case 1:
                token=2;
                goto LB_5;
              case 2:
                goto LB_5;
              default:
                {mIndex--;goto LB_0;}
                break;
            }
            break;
          case 2:
            goto LB_5;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_10:
    //10->14
    switch((c=mInput[mIndex++])<=101&&c>=48?_input__1471391470[c-48]:0)
    {
      case 1:
        break;
      case 2:
        token=2;
        goto LB_11;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_14:
    //14->18
    switch((c=mInput[mIndex++])<=57&&c>=45?_input__708474162[c-45]:0)
    {
      case 1:
        //18->15
        if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
        token=3;
        goto LB_15;
      case 2:
        token=3;
        goto LB_15;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_32:
    //32->31
    switch((c=mInput[mIndex++])<=92?_input__1383441171[c]:1)
    {
      case 1:
        goto LB_31;
      case 2:
        token=9;
        //33->31
        switch((c=mInput[mIndex++])<=92?_input__568209659[c]:1)
        {
          case 1:
            goto LB_31;
          case 2:
            goto LB_32;
          case 3:
            token=9;
            goto LB_0;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_15:
    //15->15
    switch((c=mInput[mIndex++])<=57&&c>=46?_input_1040367878[c-46]:0)
    {
      case 1:
        token=3;
        goto LB_15;
      case 2:
        //16->17
        if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
        token=3;
        goto LB_17;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_11:
    //11->14
    switch((c=mInput[mIndex++])<=101&&c>=48?_input__1471391470[c-48]:0)
    {
      case 1:
        goto LB_14;
      case 2:
        token=2;
        //12->11
        switch((c=mInput[mIndex++])<=101&&c>=48?_input__1471391470[c-48]:0)
        {
          case 1:
            goto LB_11;
          case 2:
            token=2;
            //13->11
            switch((c=mInput[mIndex++])<=101&&c>=48?_input__1471391470[c-48]:0)
            {
              case 1:
                goto LB_11;
              case 2:
                token=2;
                goto LB_11;
              default:
                {mIndex--;goto LB_0;}
                break;
            }
            break;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_17:
    //17->17
    if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
    token=3;
    goto LB_17;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  #region ε
  protected unsafe virtual Match Match_YRAN(bool close)
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
  #region {|[|NUMBER|SCIENTIFIC|UNICODE|HEX|STRING|null|true|false
  protected unsafe virtual Match Match_A5dM(bool close)
  {
    if(!close)Skip();
    ushort token=0,c=0;
    int startIndex=mIndex;
    Match match=default;
    //1->46
    switch((c=mInput[mIndex++])<=123&&c>=34?_input__1332063121[c-34]:0)
    {
      case 1:
        //46->47
        if(!((c=mInput[mIndex++])==117)){mIndex--;goto LB_0;}
        //47->48
        if(!((c=mInput[mIndex++])==108)){mIndex--;goto LB_0;}
        //48->0
        if(!((c=mInput[mIndex++])==108)){mIndex--;goto LB_0;}
        token=18;
        goto LB_0;
      case 2:
        //43->44
        if(!((c=mInput[mIndex++])==114)){mIndex--;goto LB_0;}
        //44->45
        if(!((c=mInput[mIndex++])==117)){mIndex--;goto LB_0;}
        //45->0
        if(!((c=mInput[mIndex++])==101)){mIndex--;goto LB_0;}
        token=19;
        goto LB_0;
      case 3:
        //39->40
        if(!((c=mInput[mIndex++])==97)){mIndex--;goto LB_0;}
        //40->41
        if(!((c=mInput[mIndex++])==108)){mIndex--;goto LB_0;}
        //41->42
        if(!((c=mInput[mIndex++])==115)){mIndex--;goto LB_0;}
        //42->0
        if(!((c=mInput[mIndex++])==101)){mIndex--;goto LB_0;}
        token=20;
        goto LB_0;
      case 4:
        //34->35
        if(!((c=mInput[mIndex++])==117)){mIndex--;goto LB_0;}
        //35->36
        if(!(((c=mInput[mIndex++])<=122&&c>=48?_input__621769226[c-48]:0)!=0)){mIndex--;goto LB_0;}
        //36->37
        if(!(((c=mInput[mIndex++])<=122&&c>=48?_input__621769226[c-48]:0)!=0)){mIndex--;goto LB_0;}
        //37->38
        if(!(((c=mInput[mIndex++])<=122&&c>=48?_input__621769226[c-48]:0)!=0)){mIndex--;goto LB_0;}
        //38->0
        if(!(((c=mInput[mIndex++])<=122&&c>=48?_input__621769226[c-48]:0)!=0)){mIndex--;goto LB_0;}
        token=5;
        goto LB_0;
      case 5:
        break;
      case 6:
        token=2;
        //27->29
        switch((c=mInput[mIndex++])<=120&&c>=46?_input_1848973207[c-46]:0)
        {
          case 1:
            //29->30
            if(!(((c=mInput[mIndex++])<=122&&c>=48?_input__621769226[c-48]:0)!=0)){mIndex--;goto LB_0;}
            //30->0
            if(!(((c=mInput[mIndex++])<=122&&c>=48?_input__621769226[c-48]:0)!=0)){mIndex--;goto LB_0;}
            token=6;
            goto LB_0;
          case 2:
            goto LB_14;
          case 3:
            //28->10
            if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
            token=2;
            goto LB_10;
          case 4:
            token=2;
            goto LB_5;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      case 7:
        token=2;
        //22->14
        switch((c=mInput[mIndex++])<=101&&c>=46?_input_1061576110[c-46]:0)
        {
          case 1:
            goto LB_14;
          case 2:
            //25->26
            if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
            token=2;
            //26->10
            if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
            token=2;
            goto LB_10;
          case 3:
            token=2;
            //23->24
            switch((c=mInput[mIndex++])<=101&&c>=46?_input__1448850722[c-46]:0)
            {
              case 1:
                token=2;
                //24->5
                switch((c=mInput[mIndex++])<=101&&c>=46?_input__1448850722[c-46]:0)
                {
                  case 1:
                    token=2;
                    goto LB_5;
                  case 2:
                    goto LB_5;
                  default:
                    {mIndex--;goto LB_0;}
                    break;
                }
                break;
              case 2:
                goto LB_5;
              default:
                {mIndex--;goto LB_0;}
                break;
            }
            break;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      case 8:
        //2->3
        if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
        token=2;
        //3->14
        switch((c=mInput[mIndex++])<=101&&c>=46?_input_1061576110[c-46]:0)
        {
          case 1:
            goto LB_14;
          case 2:
            //20->21
            if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
            token=2;
            //21->10
            if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
            token=2;
            goto LB_10;
          case 3:
            token=2;
            //4->19
            switch((c=mInput[mIndex++])<=101&&c>=46?_input__1448850722[c-46]:0)
            {
              case 1:
                token=2;
                //19->5
                switch((c=mInput[mIndex++])<=101&&c>=46?_input__1448850722[c-46]:0)
                {
                  case 1:
                    token=2;
                    goto LB_5;
                  case 2:
                    goto LB_5;
                  default:
                    {mIndex--;goto LB_0;}
                    break;
                }
                break;
              case 2:
                goto LB_5;
              default:
                {mIndex--;goto LB_0;}
                break;
            }
            break;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      case 9:
        token=12;
        goto LB_0;
      case 10:
        token=16;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_31:
    //31->31
    switch((c=mInput[mIndex++])<=92?_input__568209659[c]:1)
    {
      case 1:
        goto LB_31;
      case 2:
        goto LB_32;
      case 3:
        token=9;
        goto LB_0;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_5:
    //5->14
    switch((c=mInput[mIndex++])<=101&&c>=46?_input_1061576110[c-46]:0)
    {
      case 1:
        goto LB_14;
      case 2:
        //8->9
        if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
        token=2;
        //9->10
        if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
        token=2;
        break;
      case 3:
        token=2;
        //6->7
        switch((c=mInput[mIndex++])<=101&&c>=46?_input__1448850722[c-46]:0)
        {
          case 1:
            token=2;
            //7->5
            switch((c=mInput[mIndex++])<=101&&c>=46?_input__1448850722[c-46]:0)
            {
              case 1:
                token=2;
                goto LB_5;
              case 2:
                goto LB_5;
              default:
                {mIndex--;goto LB_0;}
                break;
            }
            break;
          case 2:
            goto LB_5;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_10:
    //10->14
    switch((c=mInput[mIndex++])<=101&&c>=48?_input__1471391470[c-48]:0)
    {
      case 1:
        break;
      case 2:
        token=2;
        goto LB_11;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_14:
    //14->18
    switch((c=mInput[mIndex++])<=57&&c>=45?_input__708474162[c-45]:0)
    {
      case 1:
        //18->15
        if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
        token=3;
        goto LB_15;
      case 2:
        token=3;
        goto LB_15;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_32:
    //32->31
    switch((c=mInput[mIndex++])<=92?_input__1383441171[c]:1)
    {
      case 1:
        goto LB_31;
      case 2:
        token=9;
        //33->31
        switch((c=mInput[mIndex++])<=92?_input__568209659[c]:1)
        {
          case 1:
            goto LB_31;
          case 2:
            goto LB_32;
          case 3:
            token=9;
            goto LB_0;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_15:
    //15->15
    switch((c=mInput[mIndex++])<=57&&c>=46?_input_1040367878[c-46]:0)
    {
      case 1:
        token=3;
        goto LB_15;
      case 2:
        //16->17
        if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
        token=3;
        goto LB_17;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_11:
    //11->14
    switch((c=mInput[mIndex++])<=101&&c>=48?_input__1471391470[c-48]:0)
    {
      case 1:
        goto LB_14;
      case 2:
        token=2;
        //12->11
        switch((c=mInput[mIndex++])<=101&&c>=48?_input__1471391470[c-48]:0)
        {
          case 1:
            goto LB_11;
          case 2:
            token=2;
            //13->11
            switch((c=mInput[mIndex++])<=101&&c>=48?_input__1471391470[c-48]:0)
            {
              case 1:
                goto LB_11;
              case 2:
                token=2;
                goto LB_11;
              default:
                {mIndex--;goto LB_0;}
                break;
            }
            break;
          default:
            {mIndex--;goto LB_0;}
            break;
        }
        break;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_17:
    //17->17
    if(!(((c=mInput[mIndex++])<=57&&c>=48)==true)){mIndex--;goto LB_0;}
    token=3;
    goto LB_17;
    LB_0:
    match=new Match(token,startIndex,mIndex);
    return match;
  }
  #endregion
  protected unsafe virtual void Skip()
  {
    ushort token=0,c=0;
    int startIndex=mIndex;
    //1->4
    switch((c=mInput[mIndex++])<=47&&c>=9?_input_1618766050[c-9]:0)
    {
      case 1:
        token=10;
        break;
      case 2:
        //2->3
        if(!((c=mInput[mIndex++])==47)){mIndex--;goto LB_0;}
        token=11;
        goto LB_3;
      default:
        {mIndex--;goto LB_0;}
        break;
    }
    LB_4:
    //4->4
    if(!(((c=mInput[mIndex++])<=32&&c>=9?_input__37859633[c-9]:0)!=0)){mIndex--;goto LB_0;}
    token=10;
    goto LB_4;
    LB_3:
    //3->3
    if(!(((c=mInput[mIndex++])<=10?_input_726480692[c]:1)!=0)){mIndex--;goto LB_0;}
    token=11;
    goto LB_3;
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
        return "NUMBER";
      case 3:
        return "SCIENTIFIC";
      case 5:
        return "UNICODE";
      case 6:
        return "HEX";
      case 9:
        return "STRING";
      case 10:
        return "Skip";
      case 11:
        return "Skip";
      case 12:
        return "{";
      case 13:
        return ":";
      case 14:
        return ",";
      case 15:
        return "}";
      case 16:
        return "[";
      case 17:
        return "]";
      case 18:
        return "null";
      case 19:
        return "true";
      case 20:
        return "false";
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
      case 5:
        return false;
      case 6:
        return false;
      case 9:
        return false;
      case 10:
        return true;
      case 11:
        return true;
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
      default:
        throw new ArgumentException($"字典中不包含Token为'{token}'的令牌。");
    }
  }
  protected unsafe virtual JsonItem parse(char* input,int length)
  {
    mIndex=0;
    mInput=input;
    mLength=length;
    Match readStep=default;
    JsonItem result=default;
    //1->2
    result=json();
    //2->0
    if(!((readStep=Match_YRAN(false))==1)){ReportError(readStep.SourceSpan,$"应匹配'ε'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    goto LB_0;
    LB_0:
    return result;
  }
  protected unsafe virtual JsonItem json(Match readStep=default)
  {
    JsonItem result=default;
    //3->4
    result=json_item(readStep);
    return result;
  }
  protected unsafe virtual JsonItem json_item(Match readStep=default)
  {
    JsonItem loc_6_0=default,result=default;
    switch(readStep=readStep!=0?readStep:Match_A5dM(false))
    {
      case 2:
      case 3:
      case 5:
      case 6:
      case 9:
      case 18:
      case 19:
      case 20:
        //5->6
        loc_6_0=json_prim(readStep);
        result=loc_6_0;
        break;
      case 12:
        //5->6
        loc_6_0=json_obj(readStep);
        result=loc_6_0										;
        break;
      case 16:
        //5->6
        loc_6_0=json_arr(readStep);
        result=loc_6_0;
        break;
      default:
        {ReportError(readStep.SourceSpan,$"应匹配'['时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
        break;
    }
    return result;
  }
  protected unsafe virtual JsonItem json_prim(Match readStep=default)
  {
    Match loc_1_0=default;
    JsonItem result=default;
    //7->8
    switch(loc_1_0=(readStep=readStep!=0?readStep:Match_0xEU(false)))
    {
      case 9:
        result=new JsonString(loc_1_0);
        break;
      case 6:
        result=new JsonHex(loc_1_0);
        break;
      case 5:
        result=new JsonUnicode(loc_1_0);
        break;
      case 3:
        result=new JsonScientific(loc_1_0);
        break;
      case 2:
        result=new JsonNumber(loc_1_0);
        break;
      case 20:
        result=new JsonFalse(loc_1_0);
        break;
      case 19:
        result=new JsonTrue(loc_1_0);
        break;
      case 18:
        result=new JsonNull(loc_1_0);
        break;
      default:
        {ReportError(readStep.SourceSpan,$"应匹配'null'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
        break;
    }
    return result;
  }
  protected unsafe virtual JsonItem json_arr(Match readStep=default)
  {
    JsonArrayItems loc_8_0=default;
    JsonItem result=default;
    //9->10
    if(!((readStep=readStep!=0?readStep:Match_gRxt(false))==16)){ReportError(readStep.SourceSpan,$"应匹配'['时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    //10->11
    loc_8_0=json_arr_items();
    //11->12
    if(!((readStep=Match_Q9El(false))==17)){ReportError(readStep.SourceSpan,$"应匹配']'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    result=new JsonArray(loc_8_0);
    return result;
  }
  protected unsafe virtual JsonArrayItems json_arr_items(Match readStep=default)
  {
    JsonItem loc_6_0=default,loc_2_0=default;
    JsonArrayItems loc_7_0=default,result=default;
    if(readStep=readStep!=0?readStep:Match_A5dM(false))
    {
      switch(readStep)
      {
        case 2:
        case 3:
        case 5:
        case 6:
        case 9:
        case 12:
        case 16:
        case 18:
        case 19:
        case 20:
          //13->14
          loc_6_0=json_item(readStep);
          result=loc_7_0=new JsonArrayItems(loc_6_0);
          break;
        default:
          {ReportError(readStep.SourceSpan,$"应匹配'NUMBER'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
          break;
      }
    }
    else
    {
      //13->14
      result=loc_7_0=new JsonArrayItems();
    }
    LB_14:
    //14->15
    if(!((readStep=Match_0MEZ(false))==14))goto LB_0;
    //15->14
    loc_2_0=json_item();
    result=loc_7_0.Add(loc_2_0);
    goto LB_14;
    LB_0:
    return result;
  }
  protected unsafe virtual JsonItem json_obj(Match readStep=default)
  {
    Match loc_1_0=default;
    JsonItem result=default;
    //16->17
    if(!((loc_1_0=(readStep=readStep!=0?readStep:Match_cwtp(false)))==12)){ReportError(readStep.SourceSpan,$"应匹配'{{'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    //17->18
    json_obj_mems();
    //18->19
    if(!((readStep=Match_VhQc(false))==15)){ReportError(readStep.SourceSpan,$"应匹配'}}'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    result=new JsonObject(loc_1_0);
    return result;
  }
  protected unsafe virtual JsonMembers json_obj_mems(Match readStep=default)
  {
    JsonMember loc_3_0=default,loc_5_0=default;
    JsonMembers loc_4_0=default,result=default;
    if((readStep=readStep!=0?readStep:Match_NR1Q(false))==9)
    {
      //20->21
      loc_3_0=json_obj_mem(readStep);
      result=loc_4_0=new JsonMembers(loc_3_0);
    }
    else
    {
      //20->21
      result=loc_4_0=new JsonMembers();
    }
    LB_21:
    //21->22
    if(!((readStep=Match_0MEZ(false))==14))goto LB_0;
    //22->21
    loc_5_0=json_obj_mem();
    result=loc_4_0=loc_4_0.Add(loc_5_0);
    goto LB_21;
    LB_0:
    return result;
  }
  protected unsafe virtual JsonMember json_obj_mem(Match readStep=default)
  {
    Match loc_1_0=default;
    JsonItem loc_2_0=default;
    JsonMember result=default;
    //23->24
    if(!((loc_1_0=(readStep=readStep!=0?readStep:Match_NR1Q(false)))==9)){ReportError(readStep.SourceSpan,$"应匹配'STRING'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    //24->25
    if(!((readStep=Match_NN1c(false))==13)){ReportError(readStep.SourceSpan,$"应匹配':'时扫描到了一个'{GetMatchName(readStep.Token)}'。");}
    //25->26
    loc_2_0=json_item();
    result=new JsonMember(loc_1_0, loc_2_0)		;
    return result;
  }
}