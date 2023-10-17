using System.Collections;
using System.Collections.Generic;
public interface ISerializeableProfile
{
    public void OnSerialize();
    public void OnDeserialized();
}