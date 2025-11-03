using System.ServiceModel;

namespace BusinessRules;

public static class BusinessRuleFaultFactory
{
    public static FaultException<BusinessRuleFault> ToFaultException(this BusinessRule businessRule)
    {
        FaultException<BusinessRuleFault> exception = new(
            new BusinessRuleFault(businessRule), 
            new FaultReason(businessRule.InternalRule),
            new FaultCode(businessRule.InternalKey));
        return exception;
    }
}