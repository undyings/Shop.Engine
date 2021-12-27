using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commune.Data;
using Commune.Basis;

namespace Shop.Engine
{
  public class MessageType
  {
    public readonly static FieldBlank<int> Id = new FieldBlank<int>("Id", IntLongConverter.Default);
    public readonly static FieldBlank<int> ArticleId = new FieldBlank<int>("ArticleId", IntLongConverter.Default);
    public readonly static FieldBlank<int> UserId = new FieldBlank<int>("UserId", IntLongConverter.Default);
    public readonly static FieldBlank<int?> WhomId = new FieldBlank<int?>("WhomId", IntNullableLongConverter.Default);
    public readonly static FieldBlank<string> Content = new FieldBlank<string>("Content");
    public readonly static FieldBlank<DateTime> CreateTime = new FieldBlank<DateTime>("CreateTime");
    public readonly static FieldBlank<DateTime> ModifyTime = new FieldBlank<DateTime>("ModifyTime");
    public readonly static FieldBlank<int> PlusCounter = new FieldBlank<int>("PlusCounter", IntLongConverter.Default);
    public readonly static FieldBlank<int> MinusCounter = new FieldBlank<int>("MinusCounter", IntLongConverter.Default);
    public readonly static FieldBlank<string> Xml = new FieldBlank<string>("Xml");

    public readonly static SingleIndexBlank MessageById = new SingleIndexBlank("MessageById", Id);
    public readonly static MultiIndexBlank MessagesByArticleId = new MultiIndexBlank(
      "MessagesByArticleId", ArticleId);
    public readonly static MultiIndexBlank MessagesByUserId = new MultiIndexBlank(
      "MessagesByUserId", UserId);
  }

  public class RatingType
  {
    public readonly static FieldBlank<int> Id = new FieldBlank<int>("Id", IntLongConverter.Default);
    public readonly static FieldBlank<int> ArticleId = new FieldBlank<int>("ArticleId", IntLongConverter.Default);
    public readonly static FieldBlank<int> MessageId = new FieldBlank<int>("MessageId", IntLongConverter.Default);
    public readonly static FieldBlank<int> UserId = new FieldBlank<int>("UserId", IntLongConverter.Default);
    public readonly static FieldBlank<int> Value = new FieldBlank<int>("Value", IntLongConverter.Default);
    public readonly static FieldBlank<DateTime> CreateTime = new FieldBlank<DateTime>("CreateTime");

    public readonly static SingleIndexBlank RatingById = new SingleIndexBlank("RatingById", Id);
    public readonly static MultiIndexBlank RatingsByMessageId = new MultiIndexBlank("RatingsByMessageId", MessageId);
    public readonly static MultiIndexBlank RatingsByMessageIdAndValue =
      new MultiIndexBlank("RatingsByMessageIdAndValue", MessageId, Value);
    public readonly static SingleIndexBlank RatingByMessageIdAndUserId =
      new SingleIndexBlank("RatingByMessageIdAndUserId", MessageId, UserId);
    public readonly static MultiIndexBlank RatingsByUserId = new MultiIndexBlank("RatingsByUserId", UserId);

  }
}
