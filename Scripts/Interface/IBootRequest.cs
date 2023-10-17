using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IBootRequest
{
    public async Task<bool> OnBoot() { return true; }
}
