//所有可损坏对象的接口（依赖倒置原则的强内聚，松耦合）
public interface IDamagable<DamageObject>
{
    void Hit(DamageObject DO);
}
